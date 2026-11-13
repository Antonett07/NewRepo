using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.DB.Models
{
    public class Entities
    {
        [Table("Roles")]
        public class Role
        {
            [Key]
            public int RoleID { get; set; }
            [Required, MaxLength(50)]
            public string RoleName { get; set; } = string.Empty;
            public ICollection<User> Users { get; set; } = new List<User>();
        }

        [Table("Users")]
        public class User
        {
            [Key]
            public int UserID { get; set; }
            public int RoleID { get; set; }
            [ForeignKey("RoleID")]
            public Role Role { get; set; } = null!;
            [Required, MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;
            [Required, MaxLength(50)]
            public string LastName { get; set; } = string.Empty;
            [Required, MaxLength(100)]
            public string Email { get; set; } = string.Empty;
            [Required, MaxLength(255)]
            public string PasswordHash { get; set; } = string.Empty;
            [MaxLength(20)]
            public string? PhoneNumber { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public ICollection<Event> Events { get; set; } = new List<Event>();
            public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        }

        [Table("Events")]
        public class Event
        {
            [Key]
            public int EventID { get; set; }
            public int OrganiserID { get; set; }
            [ForeignKey("OrganiserID")]
            public User Organiser { get; set; } = null!;
            [Required, MaxLength(150)]
            public string EventName { get; set; } = string.Empty;
            public string? Description { get; set; }
            [Required, MaxLength(150)]
            public string Location { get; set; } = string.Empty;
            public DateTime EventDate { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public ICollection<Category> Categories { get; set; } = new List<Category>();
        }

        [Table("Categories")]
        public class Category
        {
            [Key]
            public int CategoryID { get; set; }
            public int EventID { get; set; }
            [ForeignKey("EventID")]
            public Event Event { get; set; } = null!;
            [Required, MaxLength(100)]
            public string CategoryName { get; set; } = string.Empty;
            [Column(TypeName = "decimal(5,2)")]
            public decimal DistanceKM { get; set; }
            [Column(TypeName = "decimal(10,2)")]
            public decimal EntryFee { get; set; }
            public int MaxParticipants { get; set; }

            public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        }

        [Table("Enrolments")]
        public class Enrolment
        {
            [Key]
            public int EnrolmentID { get; set; }
            public int ParticipantID { get; set; }
            [ForeignKey("ParticipantID")]
            public User Participant { get; set; } = null!;
            public int CategoryID { get; set; }
            [ForeignKey("CategoryID")]
            public Category Category { get; set; } = null!;
            public DateTime EnrolmentDate { get; set; } = DateTime.Now;
            [Required, MaxLength(20)]
            public string PaymentStatus { get; set; } = "Pending";
            public int? RaceNumber { get; set; }

            public Result? Result { get; set; }
        }

        [Table("Results")]
        public class Result
        {
            [Key]
            public int ResultID { get; set; }
            public int EnrolmentID { get; set; }
            [ForeignKey("EnrolmentID")]
            public Enrolment Enrolment { get; set; } = null!;
            public TimeSpan? FinishTime { get; set; }
            public int? OverallPosition { get; set; }
            public int? CategoryPosition { get; set; }
            [Required, MaxLength(20)]
            public string Status { get; set; } = "Finished";
        }
    }
}
