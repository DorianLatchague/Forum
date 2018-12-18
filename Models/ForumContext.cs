using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace Forum2.Models
{
    public class ForumContext : IdentityDbContext
    {
        public ForumContext(DbContextOptions<ForumContext> options) : base(options) { }
        public DbSet<Categories> categories { get;set; }
        public DbSet<Topics> topics { get;set; }
        public DbSet<Posts> posts { get;set; }
        public DbSet<User> users { get;set; }
        public DbSet<Moderators> moderators { get;set; }
    }
    public class User : IdentityUser
    {
        [Required]
        [MinLength(2)]
        public string FirstName { get;set; }
        [Required]
        [MinLength(2)]
        public string LastName { get;set; }
        public List<Topics> Topics { get;set; }
        public List<Posts> Posts { get;set; }
        public List<Moderators> Moderating { get;set; }
        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
    }
    public class Register
    {
        [Required]
        [MinLength(4)]
        public string UserName { get;set; }
        [Required]
        [MinLength(2)]
        public string FirstName { get;set; }
        [Required]
        [MinLength(2)]
        public string LastName { get;set; }
        [Required]
        [EmailAddress]
        public string Email { get;set; }
        [Required]
        public string Password { get;set; }
        [Required]
        [Compare("Password")]
        public string PassConf { get;set; }
    }
    public class Login
    {
        [Required]
        public string UserName { get;set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get;set; }
    }
    public class EditUser
    {
        [Required]
        [MinLength(2)]
        public string FirstName { get;set; }
        [Required]
        [MinLength(2)]
        public string LastName { get;set; }
        [Required]
        [EmailAddress]
        public string Email { get;set; }
        public string Password { get;set; }
        [Compare("Password")]
        public string PassConf { get;set; }
        public string OldPassword { get;set; }
    }
    public class Categories
    {
        [Key]
        public int CategoriesId { get;set; }
        [Required]
        [MinLength(2)]
        public string Name { get;set; }
        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
        public List<Topics> Topics { get;set; }
        public List<Moderators> Moderators { get;set; }
    }
    public class Moderators
    {
        [Key]
        public int ModeratorsId { get;set; }
        public Categories Categories { get;set; }
        public int CategoriesId { get;set; }
        public User User { get;set; }
        public string UserId { get;set; }
        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
    }
    public class Topics
    {
        [Key]
        public int TopicsId { get;set; }
        [Required]
        [MinLength(5)]
        public string Title { get;set; }
        [MinLength(5)]
        public string Topic { get;set; }
        public string UserId { get;set;}
        public User User { get;set; }
        public int CategoriesId { get;set; }
        public Categories Category { get;set; }
        public List<Posts> Posts { get;set; }
        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
    }
    public class Posts
    {
        [Key]
        public int PostsId { get;set; }
        [Required]
        [MinLength(5)]
        public string Post { get;set; }
        public int TopicsId { get;set; }
        public Topics Topic { get;set; }
        public string UserId { get;set;}
        public User User { get;set; }
        public DateTime CreatedAt { get;set; } = DateTime.Now;
        public DateTime UpdatedAt { get;set; } = DateTime.Now;
    }
}