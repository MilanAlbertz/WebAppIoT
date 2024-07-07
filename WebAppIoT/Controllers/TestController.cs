using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppIoT.Hubs;

namespace WebAppIoT.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DataHub> _hubContext;

        public TestController(ApplicationDbContext context, IHubContext<DataHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index(int? gatewayId)
        {
            IQueryable<Data> query = _context.Data;

            // Filter by gateway if specified
            if (gatewayId.HasValue)
            {
                query = query.Where(d => d.GatewayId == gatewayId);
            }

            // Group by SensorId and fetch the latest record
            var dataList = query.GroupBy(d => new { d.GatewayId, d.SensorId })
                                .Select(g => g.OrderByDescending(d => d.TimeStamp).FirstOrDefault())
                                .ToList();

            // Get distinct GatewayIds for filtering
            var gatewayIds = _context.Data.Select(d => d.GatewayId)
                                          .Distinct()
                                          .ToList();

            // Calculate min and max timestamps
            var minTimestamp = query.Min(d => d.TimeStamp);
            var maxTimestamp = query.Max(d => d.TimeStamp);

            var model = new IndexViewModel
            {
                DataList = dataList,
                GatewayIds = gatewayIds,
                MinTimestamp = minTimestamp,
                MaxTimestamp = maxTimestamp
            };

            // Assuming you need to prepare data for the chart
            model.ChartData = PrepareChartData(query.ToList());

            return View(model);
        }

        // Method to prepare data for the chart
        private Dictionary<int, List<Data>> PrepareChartData(List<Data> dataList)
        {
            var chartData = new Dictionary<int, List<Data>>();

            foreach (var data in dataList)
            {
                if (!chartData.ContainsKey(data.SensorId))
                {
                    chartData[data.SensorId] = new List<Data>();
                }

                chartData[data.SensorId].Add(data);
            }

            return chartData;
        }

        [HttpPost]
        public async Task<IActionResult> AddData(Data newData)
        {
            _context.Data.Add(newData);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveDataUpdate");
            return RedirectToAction("Index");
        }
    }

    public class IndexViewModel
    {
        public List<Data> DataList { get; set; }
        public List<int> GatewayIds { get; set; }
        public DateTime MinTimestamp { get; set; }
        public DateTime MaxTimestamp { get; set; }
        public Dictionary<int, List<Data>> ChartData { get; set; } // Data for chart
    }
}