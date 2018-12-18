using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Forum2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Forum.Controllers
{
    [Authorize]
    [Route("/forum/")]
    public class ForumController : Controller
    {
        private ForumContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private async Task<User> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }
        private async Task<bool> IsAdmin(User user)
        {
            return await _userManager.IsInRoleAsync(user, "Level2");
        }
        public ForumController(
            ForumContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
            ViewBag.Categories = _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).Include(c => c.Topics);
            ViewBag.Role = false;
            if (HttpContext.User.IsInRole("Level2"))
            {
                ViewBag.Role = true;
            }
            return View();
        }
        [Authorize(Roles="Level2")]
        [HttpPost("newcategory")]
        public IActionResult CreateCategory(Categories newCategory)
        {
            if(ModelState.IsValid)
            {
                if(!_context.categories.Any(c => c.Name == newCategory.Name))
                {
                    Categories new_category = new Categories
                    {
                        Name = newCategory.Name,
                    };
                    _context.Add(new_category);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Name", "This category already exists.");
            }
            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
            ViewBag.Categories = _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).Include(c => c.Topics);
            ViewBag.Role = false;
            if (HttpContext.User.IsInRole("Level2"))
            {
                ViewBag.Role = true;
            }
            return View("Index");
        }
        [HttpGet("category/{str}")]
        public IActionResult Category(string str)
        {
            if(_context.categories.Any(c => c.Name == str))
            {
                ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                ViewBag.Category = _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Name == str);
                ViewBag.Topics = _context.topics.Include(t => t.User).Include(t => t.Category).Where(t => t.Category.Name == str);
                ViewBag.Role = false;
                if (HttpContext.User.IsInRole("Level2") || _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Name == str).Moderators.Any(m => m.User.UserName == @ViewBag.UserName))
                {
                    ViewBag.Role = true;
                }
                return View();
            }
            return RedirectToAction("Index");
        }
        [HttpPost("category/{str}/newtopic")]
        public IActionResult CreateTopic(Topics newTopic, string str)
        {
            if(ModelState.IsValid)
            {

                int cat_id = _context.categories.FirstOrDefault(c => c.Name == str).CategoriesId;
                Topics new_topic = new Topics
                {
                    Title = newTopic.Title,
                    CategoriesId = cat_id,
                    Topic = newTopic.Topic,
                    UserId = _userManager.GetUserId(HttpContext.User),
                };
                _context.Add(new_topic);
                _context.SaveChanges();
                return RedirectToAction("Category", new {str = str});
            }
            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
            ViewBag.Category = _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Name == str);
            ViewBag.Topics = _context.topics.Include(t => t.User).Include(t => t.Category).Where(t => t.Category.Name == str);
            ViewBag.Role = false;
            if (HttpContext.User.IsInRole("Level2") || _context.categories.Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Name == str).Moderators.Any(m => m.User.UserName == @ViewBag.UserName))
            {
                ViewBag.Role = true;
            }
            return View("Category");
        }
        [HttpGet("topic/{topic_id}")]
        public IActionResult Topic(int topic_id)
        {
            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
            ViewBag.Topic = _context.topics.Include(t => t.User).FirstOrDefault(t => t.TopicsId == topic_id);
            ViewBag.Posts = _context.posts.Include(p => p.User).Include(p => p.Topic).Where(t => t.Topic.TopicsId == topic_id);
            ViewBag.Role = false;
            if (HttpContext.User.IsInRole("Level2") || _context.categories.Include(c => c.Topics).Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Topics.Any(t => t.TopicsId == topic_id)).Moderators.Any(m => m.User.UserName == @ViewBag.UserName))
            {
                ViewBag.Role = true;
            }
            return View();
        }
        [HttpPost("topic/{topic_id}/newtopic")]
        public IActionResult CreatePost(Posts newPost, int topic_id)
        {
            if(ModelState.IsValid)
            {
                Posts new_post = new Posts
                {
                    Post = newPost.Post,
                    TopicsId = topic_id,
                    UserId = _userManager.GetUserId(HttpContext.User),
                };
                _context.Add(new_post);
                _context.SaveChanges();
                return RedirectToAction("Topic", new {topic_id = topic_id});
            }
            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
            ViewBag.Topic = _context.topics.Include(t => t.User).FirstOrDefault(t => t.TopicsId == topic_id);
            ViewBag.Posts = _context.posts.Include(p => p.User).Include(p => p.Topic).Where(t => t.Topic.TopicsId == topic_id);
            ViewBag.Role = false;
            if (HttpContext.User.IsInRole("Level2") || _context.categories.Include(c => c.Topics).Include(c => c.Moderators).ThenInclude(m => m.User).FirstOrDefault(c => c.Topics.Any(t => t.TopicsId == topic_id)).Moderators.Any(m => m.User.UserName == @ViewBag.UserName))
            {
                ViewBag.Role = true;
            }
            return View("Topic");
        }
        [Authorize(Roles="Level2")]
        [HttpGet("category/{category_id}/delete")]
        public IActionResult DeleteCategory(int category_id)
        {
            _context.categories.Remove(_context.categories.FirstOrDefault(c => c.CategoriesId == category_id));
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet("topic/{topic_id}/delete")]
        public IActionResult DeleteTopic(int topic_id)
        {
            var topic = _context.topics.Include(t => t.Category).FirstOrDefault(t => t.TopicsId == topic_id);
            string redirect = topic.Category.Name;
            _context.topics.Remove(topic);
            _context.SaveChanges();
            return RedirectToAction("Category", new {str = redirect});
        }
        [HttpGet("post/{post_id}/delete")]
        public IActionResult DeletePost(int post_id)
        {
            var post = _context.posts.FirstOrDefault(p => p.PostsId == post_id);
            int redirect = post.TopicsId;
            _context.posts.Remove(post);
            _context.SaveChanges();
            return RedirectToAction("Topic", new {topic_id = redirect});
        }
        [HttpGet("edit/{name}")]
        public IActionResult EditUser(string name)
        {
            if(_context.users.Any(u => u.UserName == name))
            {
                if (name == _userManager.GetUserName(HttpContext.User))
                {
                    ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                    ViewBag.User = _context.users.FirstOrDefault(u => u.UserName == name);
                    ViewBag.Self = true;
                    return View();
                }
                else if (HttpContext.User.IsInRole("Level1"))
                {
                    return RedirectToAction("Index");
                }
                else if (IsAdmin(_context.users.FirstOrDefault(u => u.UserName == name)).Result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var user = _context.users.Include(u => u.Moderating).ThenInclude(m => m.Categories).FirstOrDefault(u => u.UserName == name);
                    var categories = _context.categories;
                    Dictionary<string,bool> moderating = new Dictionary<string,bool>();
                    foreach(Categories category in categories)
                    {
                        if(user.Moderating.Any(m => m.Categories.Name == category.Name))
                        {
                            moderating[category.Name] = true;
                        }
                        else
                        {
                            moderating[category.Name] = false;
                        }
                    }
                    ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                    ViewBag.User = user;
                    ViewBag.Moderating = moderating;
                    ViewBag.Categories = categories;
                    ViewBag.Role = true;
                    return View();
                }   
            }
            return RedirectToAction("Index");
        }
        [HttpPost("user/editing/{name}")]
        public IActionResult EditingUser(EditUser edit_user, string name, string[] categories)
        {
            if(_userManager.GetUserName(HttpContext.User) == name)
            {
                if(ModelState.IsValid)
                {
                    var user = _context.users.FirstOrDefault(u => u.UserName == name);
                    if (_userManager.CheckPasswordAsync(user, edit_user.OldPassword).Result == true)
                    {
                        if(edit_user.Password != null)
                        {
                            var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;_userManager.ResetPasswordAsync(user, token, edit_user.Password);
                            if(_userManager.CheckPasswordAsync(user, edit_user.Password).Result == false)
                            {
                                ModelState.AddModelError("Password", "Your new Password is invalid.");
                                ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                                ViewBag.User = user;
                                ViewBag.Self = true;
                                return View("EditUser");
                            }
                        }
                        user.Email = edit_user.Email;
                        user.FirstName = edit_user.FirstName; 
                        user.LastName = edit_user.LastName;
                        _context.SaveChanges();
                        return RedirectToAction("EditUser", new {name = name});
                    }
                    ModelState.AddModelError("OldPassword", "Your Password did not match the records.");
                    ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                    ViewBag.User = user;
                    ViewBag.Self = true;
                    return View("EditUser");
                }
                ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                ViewBag.User = _context.users.FirstOrDefault(u => u.UserName == name);
                ViewBag.Self = true;
                return View("EditUser");
            }
            else if(!IsAdmin(_context.users.FirstOrDefault(u => u.UserName == name)).Result && HttpContext.User.IsInRole("Level2"))
            {
                if(ModelState.IsValid)
                {
                    var user = _context.users.Include(u => u.Moderating).ThenInclude(m => m.Categories).FirstOrDefault(u => u.UserName == name);
                    if(edit_user.Password != null)
                    {
                        var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                        _userManager.ResetPasswordAsync(user, token, edit_user.Password);
                        if(_userManager.CheckPasswordAsync(user, edit_user.Password).Result == false)
                        {
                            ModelState.AddModelError("Password", "This new Password is invalid.");
                            var categoriess = _context.categories;
                            Dictionary<string,bool> moderatin = new Dictionary<string,bool>();
                            foreach(Categories category in categoriess)
                            {
                                if(user.Moderating.Any(m => m.Categories.Name == category.Name))
                                {
                                    moderatin[category.Name] = true;
                                }
                                else
                                {
                                    moderatin[category.Name] = false;
                                }
                            }
                            ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                            ViewBag.User = user;
                            ViewBag.Moderating = moderatin;
                            ViewBag.Categories = categoriess;
                            ViewBag.Role = true;
                            return View("EditUser");
                        }
                    }
                    var all_categories = _context.categories;
                    Dictionary<string,bool> moderating = new Dictionary<string,bool>();
                    foreach(Categories all_category in all_categories)
                    {

                        moderating[all_category.Name] = false;
                        foreach(string category in categories)
                        {
                            if (all_category.Name == category)
                            {
                                moderating[all_category.Name] = true;
                            }
                        }
                    }
                    foreach(KeyValuePair<string, bool> entry in moderating)
                    {
                        if(entry.Value)
                        {
                            if(!user.Moderating.Any(m => m.Categories.Name == entry.Key))
                            {
                                Moderators new_moderator = new Moderators
                                {
                                    UserId = user.Id,
                                    CategoriesId = _context.categories.FirstOrDefault(c => c.Name == entry.Key).CategoriesId,
                                };
                                _context.moderators.Add(new_moderator);
                            }
                        }
                        else
                        {
                            if(user.Moderating.Any(m => m.Categories.Name == entry.Key))
                            {
                                var UserId = user.Id;
                                var category_id = _context.categories.FirstOrDefault(c => c.Name == entry.Key).CategoriesId;
                                var delete = _context.moderators.FirstOrDefault(m => m.CategoriesId == category_id && m.UserId == user.Id);
                                _context.moderators.Remove(delete);
                            }
                        }
                    }
                    user.Email = edit_user.Email;
                    user.FirstName = edit_user.FirstName; 
                    user.LastName = edit_user.LastName;
                    _context.SaveChanges();
                    return RedirectToAction("EditUser", new {name = name});
                }
                var users = _context.users.Include(u => u.Moderating).ThenInclude(m => m.Categories).FirstOrDefault(u => u.UserName == name);
                var categorie = _context.categories;
                Dictionary<string,bool> moderatings = new Dictionary<string,bool>();
                foreach(Categories category in categorie)
                {
                    if(users.Moderating.Any(m => m.Categories.Name == category.Name))
                    {
                        moderatings[category.Name] = true;
                    }
                    else
                    {
                        moderatings[category.Name] = false;
                    }
                }
                ViewBag.UserName = _userManager.GetUserName(HttpContext.User);
                ViewBag.User = users;
                ViewBag.Moderating = moderatings;
                ViewBag.Categories = categorie;
                ViewBag.Role = true;
                return View("EditUser");
            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles="Level2")]
        [HttpGet("user/{name}/delete")]
        public IActionResult DeleteUser(string name)
        {
            if(!IsAdmin(_context.users.FirstOrDefault(u => u.UserName == name)).Result)
                {
                    _context.users.Remove(_context.users.FirstOrDefault(u => u.UserName == name));
                    _context.SaveChanges();
                }
            return RedirectToAction("Index");
        }
    }
}
    

