using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace WebAppIoT.Views.Test
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        //public List<Data> DataList { get; set; }

        public void OnGet()
        {
            //DataList = _context.Data.ToList();
        }
    }
}
