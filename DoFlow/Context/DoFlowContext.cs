using System;
using DoFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace DoFlow.Context;

public class DoFlowContext : DbContext
{
    public DbSet<UserModel> Users {get;set;}
    public DoFlowContext(DbContextOptions<DoFlowContext>options) : base(options)
    {
        
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
