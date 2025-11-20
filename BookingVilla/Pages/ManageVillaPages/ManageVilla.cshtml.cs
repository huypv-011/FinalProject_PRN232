using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingVilla.DTOs;
using BookingVilla.Services.OData;
using BussinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BookingVilla.Pages.ManageVillaPages
{
    public class ManageVillaModel : PageModel
    {
        private const int PageSize = 4;
        private readonly IODataClient _odataClient;

        public List<Villa> Villas { get; set; } = new();
        public int TotalVilla { get; set; }
        public int NumberPage { get; set; }
        public int Index { get; set; } = 1;
        public string Choice { get; set; } = "price";
        [BindProperty]
        public string Search { get; set; } = string.Empty;

        public ManageVillaModel(IODataClient odataClient)
        {
            _odataClient = odataClient;
        }

        public async Task OnGetAsync(int index = 1, string? choice = null, CancellationToken cancellationToken = default)
        {
            Choice = string.IsNullOrWhiteSpace(choice) ? Choice : choice!;
            Index = Math.Max(1, index);
            await LoadVillasAsync(cancellationToken);
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id, string? choice = null, CancellationToken cancellationToken = default)
        {
            Choice = string.IsNullOrWhiteSpace(choice) ? "price" : choice!;
            Index = 1;

            // Kiểm tra xem session token có tồn tại không (nếu không thì redirect tới trang login)
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                return RedirectToPage("/Auth/Login");
            }

            var success = await _odataClient.DeleteAsync($"/odata/Villas({id})", cancellationToken);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa thành công";
            }
            else
            {
                // Nếu xóa thất bại, đưa thông tin lỗi tổng quát và hiển thị chi tiết nếu có
                TempData["ErrorMessage"] = "Xóa thất bại. Có thể bạn không có quyền hoặc villa đang có booking.";
                if (TempData.ContainsKey("ODataError"))
                {
                    TempData["ODataErrorDetail"] = TempData["ODataError"];
                }
            }

            return RedirectToPage("/ManageVillaPages/ManageVilla", new { choice = Choice });
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            Index = 1;
            Choice = "price";

            await LoadVillasAsync(cancellationToken, Search);
            return Page();
        }

        private async Task LoadVillasAsync(CancellationToken cancellationToken, string? searchTerm = null)
        {
            var skip = (Index - 1) * PageSize;
            var queryParameters = new Dictionary<string, string?>
            {
                ["$count"] = "true",
                ["$skip"] = skip.ToString(),
                ["$top"] = PageSize.ToString(),
                ["$orderby"] = GetOrderByClause(Choice),
                ["$filter"] = BuildFilter(searchTerm)
            };

            var query = QueryHelpers.AddQueryString("/odata/Villas", queryParameters);
            try
            {
                var response = await _odataClient.GetAsync<ODataListResponse<Villa>>(query, cancellationToken);

                Villas = response?.Value ?? new List<Villa>();
                TotalVilla = response?.Count ?? Villas.Count;
                NumberPage = (int)Math.Ceiling(TotalVilla / (double)PageSize);
            }
            catch (HttpRequestException ex)
            {
                // Nếu OData trả lỗi (ví dụ 400 do $orderby Price không tồn tại trong EDM), tránh crash trang.
                Villas = new List<Villa>();
                TotalVilla = 0;
                NumberPage = 0;
                TempData["ODataError"] = ex.Message;
            }
        }

        private static string BuildFilter(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return "Status eq true";
            }

            var normalized = EscapeODataLiteral(searchTerm.Trim().ToLowerInvariant());
            return $"Status eq true and contains(tolower(Name),'{normalized}')";
        }

        private static string GetOrderByClause(string choice) =>
            choice switch
            {
                "people" => "AmountOfPeople desc",
                "room" => "AmountOfRoom desc",
                _ => "IdVilla desc"
            };

        private static string EscapeODataLiteral(string value) => value.Replace("'", "''");
    }

}
