using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zugether.DTO;
using Zugether.Models;


namespace Zugether.Controllers
{
    public class AdminController : Controller
    {
        private ZugetherContext _context;
        public AdminController(ZugetherContext context)
        {
            _context = context;
        }

        // 登入畫面
        public IActionResult Index()
        {
            ViewBag.show = false;
            return View();
        }
        // 送出使用者名稱和密碼
        [HttpPost]
        public async Task<IActionResult> Index(string username , string password)
        {
            var res = await (from a in _context.Admin
                       where a.userName == username && a.password == password
                       select a).SingleOrDefaultAsync();
            if (res == null) {
                ViewBag.show = true;
                ViewBag.color = "danger";
                ViewBag.message = "登入失敗";
                return View();
            }

            // 登入成功，設定 isLogin 為 true
            HttpContext.Session.SetString("admin_isLogin", "true");
            ViewBag.show = true;
            ViewBag.color = "success";
            ViewBag.message = "登入成功";
            return RedirectToAction("Members");
            
        }

        // 登出
        [HttpPost]
        public IActionResult Logout()
        {
            // 移除登入狀態的 Session 資訊
            HttpContext.Session.Remove("admin_isLogin");

            // 重定向至登入頁面
            return Redirect("/Admin/Index");
        }

        // 判斷是否登入 function
        public bool isLogin()
        {
            return HttpContext.Session.GetString("admin_isLogin") == "true";
        }
        
        // 會員列表
        public IActionResult Members(int page = 1, int pageSize = 3)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index"); ;
            }
            List<Member> MemberList = (from members in _context.Member
                                   select members).ToList();
            return View(MemberList);
        }
        //編輯會員頁面
        public IActionResult MemberEdit(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var member = _context.Member.Find(id);
            return View(member);
        }
        // 送出編輯資料
        [HttpPost]
        public IActionResult ConfirmMemberEdit(short member_id,string gender,DateOnly birthday)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var member = _context.Member.Find(member_id);
            member!.gender = gender;
            member.birthday = birthday;
            _context.SaveChanges();
            return Redirect("/Admin/Members");
        }
        // 刪除會員頁面
        public IActionResult MemberDelete(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var member = _context.Member.Find(id);
            return View(member);
        }
        // 刪除會員資料
        [HttpPost]
        public async Task<IActionResult> ConfirmMemberDelete(short member_id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var member = _context.Member.Find(member_id);
            var favorList = _context.Favor_List.Where(f=>f.member_id == member_id);
            if (favorList.Any())
            {
                _context.Favor_List.RemoveRange(favorList);
            }
            _context.Member.Remove(member!);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/Members");
        }
        // 房間列表
        public IActionResult Rooms()
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            List<Room> roomList = (from rooms in _context.Room
                        select rooms).ToList();
            return View(roomList);
        }
        // 編輯房間頁面
        public IActionResult RoomEdit(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            //var room = _context.Room.Find(id);
            var room  = (from r in _context.Room
                        join l in _context.Landlord on r.landlord_id equals l.landlord_id
                         where r.room_id == id
                         select new
                        {
                            r.room_id,r.room_title,r.post_date,r.isEnabled,l.consent_photo
                        }).SingleOrDefault();
            if (room?.consent_photo == null)
			{
                ViewBag.consentImage = "";
            }
            else
            {
                var base64Image = Convert.ToBase64String(room.consent_photo);
                ViewBag.consentImage = $"data:image/png;base64,{base64Image}";
                
            }
            // Pass data to the view using a ViewModel
            var viewModel = new RoomEditViewModel
            {
                room_id = room.room_id,
                room_title = room.room_title,
                post_date = room.post_date,
                isEnabled = room.isEnabled,
                consentImage = ViewBag.consentImage
            };
            return View(viewModel);
        }

        // 送出編輯資料
        [HttpPost]
        public IActionResult RoomEdit(short room_id, bool isEnabled)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var room = _context.Room.Find(room_id);
            room!.isEnabled = isEnabled ? true:false;
            _context.SaveChanges();
            return Redirect("/Admin/Rooms");
        }

        // 刪除房間頁面
        public IActionResult RoomDelete(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var room = _context.Room.Find(id);
            return View(room);
        }
        // 刪除房間資料
        [HttpPost]
        public async Task<IActionResult> ConfirmRoomDelete(short room_id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var room = _context.Room.Find(room_id);
            _context.Room.Remove(room);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/Rooms");
        }

        // 聯絡我們列表
        public IActionResult Contacts()
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index"); ;
            }
            List<Contact_us> ContactList = (from contacts in _context.Contact_us
                                       select contacts).ToList();
            return View(ContactList);
        }
        //聯絡我們頁面
        public IActionResult ContactView(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var contact = _context.Contact_us.Find(id);
            return View(contact);
        }
        // 刪除聯絡我們頁面
        public IActionResult ContactDelete(short id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var contact = _context.Contact_us.Find(id);
            return View(contact);
        }
        // 刪除房間資料
        [HttpPost]
        public async Task<IActionResult> ConfirmContactDelete(short contact_id)
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index");
            }
            var contact = _context.Contact_us.Find(contact_id);
            _context.Contact_us.Remove(contact);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/Contacts");
        }

        // 統計
        public IActionResult Analytics()
        {
            if (!isLogin())
            {
                // 若未登入，返回重定向至 Index
                return Redirect("/Admin/Index"); ;
            }
            List<Member> MemberList = (from members in _context.Member
                                       select members).ToList();
            return View(MemberList);
        }
    }
}
