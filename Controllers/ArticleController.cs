using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Zugether.Models;

namespace Zugether.Controllers
{
    public class ArticleController : Controller
    {
        // GET: /Articles/Index
        [HttpGet]
        public IActionResult Index()
        {
            // 1. 找到 JSON 檔案的路徑
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "Articles", "articleContent.json");

            // 2. 讀取 JSON 檔案內容
            string jsonData = System.IO.File.ReadAllText(filePath);

            // 3. 將 JSON 資料轉為 List<Article>
            List<ArticleClass>? articles = JsonSerializer.Deserialize<List<ArticleClass>>(jsonData);
            if (articles == null)
            {
                // 處理錯誤（返回一個包含錯誤信息的視圖或重定向）
                return View("Error"); // 根據需要創建一個錯誤視圖
            }

            // 4. 將資料傳遞給 View
            return View(articles);
        }

        public IActionResult Content(int id)
        {
            // 1. 找到 JSON 檔案的路徑
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "Articles", "articleContent.json");
            // 2. 讀取 JSON 檔案內容
            string jsonData = System.IO.File.ReadAllText(filePath);
            // 3. 將 JSON 資料轉為 List<ArticleClass>
            List<ArticleClass>? articles = JsonSerializer.Deserialize<List<ArticleClass>>(jsonData);
            if (articles == null)
            {
                return View(new List<ArticleClass>()); // 返回空列表，避免顯示錯誤
            }

            //// 4. 找到指定 ID 的文章
            ArticleClass? selectedArticle = articles.FirstOrDefault(a => a.Id == id);
            if (selectedArticle == null)
            {
                return View("Error"); // 若找不到對應文章，返回錯誤頁面
            }
            // 4. 將資料傳遞給 View
            return View(selectedArticle); // 傳遞找到的文章到 View
        }
    }
}
