﻿using GraduationProject.DAL.Repository;
using GraduationProject.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject.DAL.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context , ICategoryRepo categoryRepo)
    {
        _context = context;
        Categoryrepo = categoryRepo;
    }
    public ICategoryRepo Categoryrepo { get; } 

    //ICategoryRepo IUnitOfWork.categoryRepo => throw new NotImplementedException();
}
