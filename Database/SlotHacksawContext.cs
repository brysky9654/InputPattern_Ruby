using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InputPattern.Database;

public partial class SlotHacksawContext : DbContext
{
    public SlotHacksawContext(DbContextOptions<SlotHacksawContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgentCall> AgentCalls { get; set; }

    public virtual DbSet<AgentJackpot> AgentJackpots { get; set; }

    public virtual DbSet<AgentRtp> AgentRtps { get; set; }

    public virtual DbSet<Api> Apis { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<SessionKey> SessionKeys { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wager> Wagers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Korean_Wansung_CS_AS");

        modelBuilder.Entity<AgentCall>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AgentJac__3214EC07424F3A81");

            entity.Property(e => e.BetAmount)
                .HasComment("Bet Money")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CallMoney)
                .HasComment("Reserved Money")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MissedMoney)
                .HasComment("Missed Money")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasComment("0:reserve,1:apply,2:cancel");
            entity.Property(e => e.Type)
                .HasDefaultValue((byte)1)
                .HasComment("1:free,2:buy");

            entity.HasOne(d => d.Game).WithMany(p => p.AgentCalls)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AgentCalls_Game");

            entity.HasOne(d => d.User).WithMany(p => p.AgentCalls)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AgentCalls_User");
        });

        modelBuilder.Entity<AgentJackpot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SessionK__3214EC07A925E04B");

            entity.ToTable(tb => tb.HasComment("There is Agent' Jackpot money.\r\nIf user bet in round, bet money's *percent is added jackpot money.\r\nAnd then this money is gived to random user in random time."));

            entity.Property(e => e.AgentCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasComment("1:reserving,2:calling");

            entity.HasOne(d => d.Api).WithMany(p => p.AgentJackpots)
                .HasForeignKey(d => d.ApiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AgentJackpots_Api");
        });

        modelBuilder.Entity<AgentRtp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AgentCal__3214EC0781B3EFE6");

            entity.ToTable("AgentRTPs", tb => tb.HasComment("There is Agent's Rtp.\r\nthis is setted by admin page."));

            entity.Property(e => e.AgentCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");

            entity.HasOne(d => d.Api).WithMany(p => p.AgentRtps)
                .HasForeignKey(d => d.ApiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AgentCall__ApiId__379B24DB");
        });

        modelBuilder.Entity<Api>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users_co__3214EC07C63E462B");

            entity.HasIndex(e => e.ApiCode, "IX_Apis_ApiCode").IsUnique();

            entity.Property(e => e.ApiCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.CallbackUrl)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.SecretKey)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("USD");
            entity.Property(e => e.Symbol)
                .HasMaxLength(10)
                .HasComment("$");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.GameCode).HasMaxLength(100);
            entity.Property(e => e.GameName).HasDefaultValue("");
            entity.Property(e => e.InitPattern).HasDefaultValue("");
            entity.Property(e => e.Memo).HasDefaultValue("");
            entity.Property(e => e.Status).HasComment("/// 0: testing\r\n/// 1: new\r\n/// 2: normal\r\n/// 3: maintence\r\n");
            entity.Property(e => e.Thumbnail).HasDefaultValue("");
            entity.Property(e => e.Type).HasComment("/// 0: blocked\r\n/// 1: normal\r\n/// 2: special\r\n/// 3: option");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__players__3213E83F795F3CB9");

            entity.HasIndex(e => e.UserId, "gameCode");

            entity.Property(e => e.Connected).HasComment("0:disconnected,1:connected");
            entity.Property(e => e.GameId).HasDefaultValueSql("('')");
            entity.Property(e => e.LastBet).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastPattern).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.LastWin).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Machine)
                .HasDefaultValue("")
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.ReplayLogList).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Settings).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.TotalCall).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalCredit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDebit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalJackpot).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalWin)
                .HasComment("Added because the original Free first spin win value is forgotten while looking for another pattern in FSOption.")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VirtualBet)
                .HasComment("Previous bets stored to calculate multi in free pattern")
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Game).WithMany(p => p.Players)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Players_Game");

            entity.HasOne(d => d.User).WithMany(p => p.Players)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Players_Users");
        });

        modelBuilder.Entity<SessionKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Settings");

            entity.Property(e => e.ExpireDateTime).HasColumnType("datetime");
            entity.Property(e => e.SessionKey1)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnType("text")
                .HasColumnName("SessionKey");
            entity.Property(e => e.SessionKeyV2)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasColumnType("text");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F03BB491A");

            entity.HasIndex(e => e.UserCode, "IX_Users_UserCode");

            entity.Property(e => e.AgentCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NickName)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.UserCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");

            entity.HasOne(d => d.Api).WithMany(p => p.Users)
                .HasForeignKey(d => d.ApiId)
                .HasConstraintName("FK__Users__ApiId__151102AD");

            entity.HasOne(d => d.Currency).WithMany(p => p.Users)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Currencies");
        });

        modelBuilder.Entity<Wager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users_co__3214EC07D476A7EE");

            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Detail).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.GameCode)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.PayoutAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasComment("0: debit 1: credit 2:cancel");

            entity.HasOne(d => d.User).WithMany(p => p.Wagers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wagers_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
