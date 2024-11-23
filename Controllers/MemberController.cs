using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zugether.DTO;
using Zugether.Models;
namespace Zugether.Controllers
{

    public class MemberController : Controller
    {
        private readonly ZugetherContext _context;
        private readonly PasswordHasher<Member> _passwordHasher;
        public MemberController(ZugetherContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Member>();
        }
        private string memberEmail()
        {
            string? isLogin = HttpContext.Session.GetString("isLogin");
            if (isLogin == "true")
            {
                return HttpContext.Session.GetString("memberEmail") ?? HttpContext.Session.GetString("googleEmail") ?? string.Empty;
            }
            return string.Empty;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Notify()
        {
            string? userName = HttpContext.Session.GetString("memberName") ?? HttpContext.Session.GetString("googleName");
            if (userName == null)
            {
                return Redirect("/Home/Index");
            }
            return View();

        }


        // 修改密碼
        public IActionResult EditPassword()
        {
            ViewBag.show = false;
            var member = (from i in _context.Member
                          where i.email == memberEmail()
                          select i).SingleOrDefault();
            return View(member);
        }

        [HttpPost]
        public async Task<IActionResult> EditPassword(string email, string password)
        {
            var member = (from i in _context.Member
                          where i.email == email
                          select i).SingleOrDefault();

            if (member == null)
            {
                ViewBag.color = "danger";
                ViewBag.show = true;
                ViewBag.message = "修改密碼失敗，請重新輸入";
                return View(member);
            };
            if (member.password != null)
            {
                //將密碼轉成雜湊
                member.password = _passwordHasher.HashPassword(member, password!);
            }
            _context.Update(member);
            await _context.SaveChangesAsync();
            ViewBag.color = "success";
            ViewBag.show = true;
            ViewBag.message = "修改密碼成功";
            return View(member);
        }

        // 修改會員資料
        public IActionResult EditInfo()
        {
            ViewBag.show = false;
            var member = (from i in _context.Member
                          where i.email == memberEmail()
                          select i).SingleOrDefault();
            if (member.avatar != null)
            {
                // Convert avatar to base64 string
                var base64Avatar = Convert.ToBase64String(member.avatar);
                ViewBag.AvatarImage = $"data:image/png;base64,{base64Avatar}";
            }
            else
            {
                ViewBag.AvatarImage = $"~/images//peopleImg.png";
            }

            //else
            //{
            //    // Default image if no avatar exists
            //    ViewBag.AvatarImage = Url.Content("~/images/peopleImg.png");
            //}
            return View(member);
        }
        [HttpPost]
        public async Task<IActionResult> EditInfo(Member edit, IFormFile avatar)
        {
            var member = (from i in _context.Member
                          where i.email == memberEmail()
                          select i).SingleOrDefault();
            if (member == null)
            {
                ViewBag.color = "danger";
                ViewBag.show = true;
                ViewBag.message = "修改會員資料失敗，請重新輸入";
                return View(member);
            }

            if (avatar != null)
            {
                using (var ms = new MemoryStream())
                {
                    await avatar.CopyToAsync(ms);
                    member.avatar = ms.ToArray();
                    var base64Avatar = Convert.ToBase64String(member.avatar);
                    ViewBag.AvatarImage = $"data:image/png;base64,{base64Avatar}";
                }
            }
            else
            {
                // 沒有上傳新頭像時，沿用舊的頭像
                if (member.avatar != null && member.avatar.Length > 0)
                {
                    var base64Avatar = Convert.ToBase64String(member.avatar);
                    ViewBag.AvatarImage = $"data:image/png;base64,{base64Avatar}";
                }
                else
                {
                    // 沒有舊頭像，使用預設圖片
                    ViewBag.AvatarImage = Url.Content("~/images//peopleImg.png");
                }
            }
            member.name = edit.name;
            member.nickname = edit.nickname;
            member.phone = edit.phone;
            member.job = edit.job;
            member.jobtime = edit.jobtime;
            member.introduce = edit.introduce;
            _context.Update(member);
            await _context.SaveChangesAsync();
            ViewBag.color = "success";
            ViewBag.show = true;
            ViewBag.message = "修改成功 !";
            return View(member);
        }


        public IActionResult AddRoom()
        {
            string? userName = HttpContext.Session.GetString("memberName") ?? HttpContext.Session.GetString("googleName");
            if (userName == null)
            {
                return Redirect("/Home/Index");
            }
            ViewBag.isAdd = true;
            return View();
        }
        public IActionResult DeleteRoom()
        {
            string? userName = HttpContext.Session.GetString("memberName") ?? HttpContext.Session.GetString("googleName");
            if (userName == null)
            {
                return Redirect("/Home/Index");
            }
            ViewBag.isAdd = false;
            return View("AddRoom");
        }

        public IActionResult EditRoom()
        {
            string? userName = HttpContext.Session.GetString("memberName") ?? HttpContext.Session.GetString("googleName");
            if (userName == null)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }

        //會員收藏
        public async Task<IActionResult> FavoriteRoom()
        {
            int? memberID = HttpContext.Session.GetInt32("FavoriteMemberID");
            IQueryable<Room> query = from x in _context.Favor_List
                                     where x.member_id == memberID
                                     join y in _context.Favorites on x.favor_list_id equals y.favor_list_id
                                     join z in _context.Room on y.room_id equals z.room_id
                                     select z;

            List<RoomViewModel> result = await query.Select(room => new RoomViewModel
            {
                Room = room,
                deviceList = _context.Device_List
                        .Where(x => x.device_list_id == room.device_list_id)
                        .Select(x => new DeviceList
                        {
                            canPet = x.keep_pet,
                            canSmoking = x.smoking
                        }).ToList(),
                roomImages = (from x in _context.Room
                              where x.room_id == room.room_id
                              join y in _context.Photo on x.album_id equals y.album_id
                              select new RoomImages
                              {
                                  room_photo = y.room_photo,
                                  photo_type = y.photo_type
                              }).ToList()
            }).ToListAsync();

            // 判斷是否有收藏的房間
            if (result.Count <= 0)
            {
                ViewBag.Message = "目前尚無收藏項目";
                return View(new List<RoomViewModel>());
            }

            // 將查詢結果傳遞到 View 中顯示
            return View(result);
        }
        //送會員編號
        [HttpPost]
        public IActionResult FavoriteMemberID(short memberID)
        {
            HttpContext.Session.SetInt32("FavoriteMemberID", memberID);
            return Ok();
        }
        //ADD收藏
        [HttpPost]
        public async Task<IActionResult> FavoriteRoom(short roomID, short memberID)
        {
            if (roomID == 0)
            {
                return Json(new { state = false, message = "無效的房間ID" });
            }
            //Favor_List要先有對應的會員編號
            //建立Favorites物件
            //找出對應的favor_list_id
            short favorListID = await (from x in _context.Favor_List
                                       where x.member_id == memberID
                                       select x.favor_list_id
                                ).FirstOrDefaultAsync();
            Favorites favorite = new Favorites
            {
                room_id = roomID,
                favor_list_id = favorListID
            };
            try
            {
                await _context.Favorites.AddAsync(favorite);
                await _context.SaveChangesAsync();
                return Json(new { state = true, message = "收藏成功" });
            }
            catch (Exception ex)
            {
                return Json(new { state = false, message = "收藏失敗" + ex.InnerException?.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFavoriteRoom(short roomID, short memberID)
        {
            Favorites? favorite = await (from x in _context.Favor_List
                                         where x.member_id == memberID
                                         join y in _context.Favorites on x.favor_list_id equals y.favor_list_id
                                         where y.room_id == roomID
                                         select y).FirstOrDefaultAsync();
            try
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { state = true, message = "成功刪除收藏" });
            }
            catch (Exception ex)
            {
                return Json(new { state = false, message = "刪除失敗" + ex.InnerException?.Message });
            }
        }
        //Alert動畫
        public IActionResult Alert(string color, string alertText, bool show, int time)
        {
            var model = new PartialAlert
            {
                Color = color,
                AlertText = alertText,
                Show = show,
                Time = time
            };
            return PartialView("_PartialAlert", model);
        }
        [HttpPost]
        public async Task<IActionResult> CheckEnabled(short roomID)
        {
            try
            {
                bool? isEnabled = await _context.Room
                                              .Where(x => x.room_id == roomID)
                                              .Select(x => x.isEnabled)
                                              .FirstOrDefaultAsync();
                return Json(isEnabled ?? false);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "伺服器錯誤：" + ex.Message);
            }
        }

    }
}
