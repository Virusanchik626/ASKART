using System;
using System.Collections.Generic;
using ASKART.API.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using Task = ASKART.API.Models.Task;

namespace ASKART.API.Data;

public partial class AskartContext : DbContext
{
    public AskartContext()
    {
    }

    public AskartContext(DbContextOptions<AskartContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Chatmember> Chatmembers { get; set; }

    public virtual DbSet<KanbanColumn> KanbanColumns { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAssignee> TaskAssignees { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=askart;password=1234;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.IdChat).HasName("PRIMARY");

            entity.ToTable("chats");

            entity.Property(e => e.IdChat)
                .ValueGeneratedNever()
                .HasColumnName("id_chat");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.TypeChat)
                .HasColumnType("enum('Личный','Группа')")
                .HasColumnName("type_chat");
            entity.Property(e => e.TypeGroup)
                .HasColumnType("enum('Личная','Рабочая')")
                .HasColumnName("type_group");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Chatmember>(entity =>
        {
            entity.HasKey(e => new { e.ChatId, e.UserId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("chatmembers");

            entity.HasIndex(e => e.ChatId, "chat_idx");

            entity.HasIndex(e => e.UserId, "user_idx");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Role)
                .HasMaxLength(45)
                .HasColumnName("role");

            entity.HasOne(d => d.Chat).WithMany(p => p.Chatmembers)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat");

            entity.HasOne(d => d.User).WithMany(p => p.Chatmembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user");
        });

        modelBuilder.Entity<KanbanColumn>(entity =>
        {
            entity.HasKey(e => new { e.IdColumn, e.ChatId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("kanban_columns");

            entity.HasIndex(e => e.ChatId, "chat_id");

            entity.Property(e => e.IdColumn).HasColumnName("id_column");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Position).HasColumnName("position");

            entity.HasOne(d => d.Chat).WithMany(p => p.KanbanColumns)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("kanban_columns_ibfk_1");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PRIMARY");

            entity.ToTable("messages");

            entity.HasIndex(e => e.ChatId, "chat1_idx");

            entity.HasIndex(e => e.SenderId, "user1_idx");

            entity.Property(e => e.IdMessage)
                .ValueGeneratedNever()
                .HasColumnName("id_message");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.EditTime)
                .HasColumnType("datetime")
                .HasColumnName("edit_time");
            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.SendTime)
                .HasColumnType("datetime")
                .HasColumnName("send_time");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Text)
                .HasMaxLength(200)
                .HasColumnName("text");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat1");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user1");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.IdTask).HasName("PRIMARY");

            entity.ToTable("tasks");

            entity.HasIndex(e => e.ChatId, "chat2_idx");

            entity.HasIndex(e => e.CreatorId, "user_idx");

            entity.Property(e => e.IdTask)
                .ValueGeneratedNever()
                .HasColumnName("id_task");
            entity.Property(e => e.AssigneUserId).HasColumnName("assigne_user_id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.Describe)
                .HasMaxLength(45)
                .HasColumnName("describe");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Priority)
                .HasColumnType("enum('Низкий','Средний','Высокий')")
                .HasColumnName("priority");
            entity.Property(e => e.Status)
                .HasColumnType("enum('Новая','В работе','Выполнена')")
                .HasColumnName("status");

            entity.HasOne(d => d.Chat).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat2");

            entity.HasOne(d => d.Creator).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user2");
        });

        modelBuilder.Entity<TaskAssignee>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("task_assignees");

            entity.HasIndex(e => e.TaskId, "task_idx");

            entity.HasIndex(e => e.UserId, "user5_idx");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany()
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user5");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("id_user");
            entity.Property(e => e.Avatar)
                .HasColumnType("mediumblob")
                .HasColumnName("avatar");
            entity.Property(e => e.DataReg)
                .HasColumnType("datetime")
                .HasColumnName("data_reg");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Lastname)
                .HasMaxLength(45)
                .HasColumnName("lastname");
            entity.Property(e => e.Login)
                .HasMaxLength(45)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Number)
                .HasMaxLength(45)
                .HasColumnName("number");
            entity.Property(e => e.Password)
                .HasMaxLength(45)
                .HasColumnName("password");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .HasColumnName("surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
