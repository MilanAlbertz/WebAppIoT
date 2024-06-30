using Microsoft.AspNetCore.Mvc;
using WebAppIoT;

namespace WebAppIoT.Controllers
{

    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
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
    }
    public class IndexViewModel
    {
        public List<Data> DataList { get; set; }
        public List<int> GatewayIds { get; set; }
    }
}