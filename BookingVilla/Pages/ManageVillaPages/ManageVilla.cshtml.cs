using BussinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;

namespace BookingVilla.Pages.ManageVillaPages
{
    public class ManageVillaModel : PageModel
    {
        private readonly IVillaRepositories _villaRepo;

        public List<Villa> Villas { get; set; }
        public int TotalVilla { get; set; }
        public int NumberPage { get; set; }
        public int Index { get; set; }
        public string Choice { get; set; }
        [BindProperty]
        public string Search { get; set; }

        public ManageVillaModel(IVillaRepositories villaRepo)
        {
            _villaRepo = villaRepo;
        }

        public void OnGet(int index = 1, string choice = null)
        {
            Choice = choice ?? Choice;
            Index = index;

            Villas = Choice switch
            {
                "people" => _villaRepo.GetAllVillasByPeople(index),
                "room" => _villaRepo.GetAllVillasByRoom(index),
                _ => _villaRepo.GetAllVillasByPrice(index)
            };

            TotalVilla = _villaRepo.GetNumberTotalVilla();
            NumberPage = _villaRepo.GetNumberVilla();
        }
        public IActionResult OnGetDelete(int id, string choice)
        {
            Choice = choice ?? Choice;
            Index = 1;
            bool result = _villaRepo.GetVillaConflict(id);
            TempData["SuccessMessage"] = result ? "ok" : "fail";

            Villas = Choice switch
            {
                "people" => _villaRepo.GetAllVillasByPeople(Index),
                "room" => _villaRepo.GetAllVillasByRoom(Index),
                _ => _villaRepo.GetAllVillasByPrice(Index)
            };

            TotalVilla = _villaRepo.GetNumberTotalVilla();
            NumberPage = _villaRepo.GetNumberVilla();
            return RedirectToPage("/ManageVillaPages/ManageVilla", new { choice });
        }
        public IActionResult OnPost()
        {
            Villas = _villaRepo.SearchByName(Search);
            Index = 1;
            TotalVilla = Villas.Count;
            NumberPage = (TotalVilla / 4) + (TotalVilla % 4 > 0 ? 1 : 0);
            return Page();
        }
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostAddVillaAsync([FromForm] VillaModel model, IFormFile Image)
        {
            Console.WriteLine($"Content-Type: {Request.ContentType}");

            if (!Request.HasFormContentType)
            {
                return BadRequest("Request is not form-data");
            }

            if (model == null)
            {
                return BadRequest("Model binding failed");
            }

            if (Image == null)
            {
                return BadRequest("Image file missing");
            }

            Console.WriteLine($"Received: {model.Name}, {model.Price}");
            return new JsonResult(new { success = true });
        }

        //public async Task<IActionResult> OnPostAddVillaAsync([FromForm] VillaModel model, IFormFile Image)
        //{
        //    var name = Request.Form["Name"];
        //    var description = Request.Form["Description"];
        //    var people = int.Parse(Request.Form["People"]);
        //    var room = int.Parse(Request.Form["Room"]);

        //    var price = double.Parse(Request.Form["Amount"]);
        //    var fromDate = DateTime.Parse(Request.Form["FromDate"]);
        //    var toDate = DateTime.Parse(Request.Form["ToDate"]);

        //    var imageFile = Request.Form.Files["Image"];
        //    var fileName = Path.GetFileName(imageFile.FileName);
        //    // Xử lý file ảnh nếu có
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        var filePath = Path.Combine("wwwroot/images/Villa", fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await imageFile.CopyToAsync(stream);
        //        }
        //    }
        //    var villa = new Villa
        //    {
        //        Name = name,
        //        Describe = description,
        //        AmountOfPeople = people,
        //        AmountOfRoom = room,
        //    };
        //    int idvilla = _villaRepo.AddVilla(villa);
        //    var imagevilla = new ImageVilla
        //    {
        //        IdVilla = idvilla,
        //        Image = fileName
        //    };
        //    _villaRepo.AddImageVilla(imagevilla);
        //    var pricevilla = new PriceVilla
        //    {
        //        FromDate = fromDate,
        //        ToDate = toDate,
        //        IdVilla = idvilla,
        //        PriceDay = price
        //    };
        //    _villaRepo.AddPriceVilla(pricevilla);
        //    return new JsonResult(new { success = true });
        //}
        public class VillaModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public decimal Price { get; set; }
            public int People { get; set; }
            public int Room { get; set; }
        }
    }

}
