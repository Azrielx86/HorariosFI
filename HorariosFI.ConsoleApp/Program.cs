using HorariosFI.Core;

int class_code = 1537;

var (clase, _) = await FIScrapper.GetClassList(class_code);

var classdict = new Dictionary<string, IEnumerable<ClassModel>>() { { class_code.ToString(), clase } };

var mp = new MPScrapper();

//var clases = new Dictionary<int, IEnumerable<ClassModel>>()
//{
//    { 119, new List<ClassModel>(){
//        new ClassModel()
//        {
//            Name = "MARGARITA CARRERA FOURNIER",
//            Grade = 4
//        },
//        new ClassModel()
//        {
//            Name = "ALAN URIEL GIL CASAS",
//            Grade = 10
//        },
//        new ClassModel()
//        {
//            Name = "GUILLERMO FERNANDEZ ANAYA",
//            Grade = 6.9
//        }
//        }
//    },
//    { 48, new List<ClassModel>(){
//        new ClassModel()
//        {
//            Name = "MARGARITA CARRERA FOURNIER",
//        }
//        }
//    },
//};

int progress = 0;
foreach (var item in classdict)
{
    mp.Run(item.Value, ref progress);
}

ExcelExport.Export(classdict);