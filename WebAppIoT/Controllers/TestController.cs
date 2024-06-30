using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
            var dataList = _context.Data
                .Where(d => gatewayId == null || d.GatewayId == gatewayId)
                .GroupBy(d => new { d.GatewayId, d.SensorId })
                .Select(g => g.OrderByDescending(d => d.TimeStamp).FirstOrDefault())
                .ToList();

            var gatewayIds = _context.Data
                .Select(d => d.GatewayId)
                .Distinct()
                .ToList();

            var model = new IndexViewModel
            {
                DataList = dataList,
                GatewayIds = gatewayIds
            };

            return View(model);
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
    }
}