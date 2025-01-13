using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using HorariosFI.Core;
using HorariosFI.Core.Models;

namespace HorariosFI.App.Models;

public class InputClassModel
{
    public int Clave { get; set; }
    public string? Nombre { get; set; }
    public List<ClassModel>? Horarios { get; set; }
}