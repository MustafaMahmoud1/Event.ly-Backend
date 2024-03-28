﻿using GraduationProject.BL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject.BL.Managers;

public interface ICategoryManager
{
    IEnumerable<CategoryReadDto> GetAll();

    CategoryReadDto? GetByName(string name);

    int Add(CategoryAddDto categoryAddDto);

    bool Delete(int id);


}