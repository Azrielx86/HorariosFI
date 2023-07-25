using HorariosFI.Core;

namespace HorariosFI.ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //string[] names = { "Ernesto Alcántara Concepción", "ROSALBA MARTINEZ LOPEZ", "Alan Gil uriel Casas" };
            var mp = new MPScrapper();

            var clases = new Dictionary<int, IEnumerable<ClassModel>>()
            {
                { 119, new List<ClassModel>(){
                    new ClassModel()
                    {
                        Name = "MARGARITA CARRERA FOURNIER",
                        Grade = 4
                    },
                    new ClassModel()
                    {
                        Name = "ALAN URIEL GIL CASAS",
                        Grade = 10
                    },
                    new ClassModel()
                    {
                        Name = "GUILLERMO FERNANDEZ ANAYA",
                        Grade = 6.9
                    }
                    }
                },
                { 48, new List<ClassModel>(){
                    new ClassModel()
                    {
                        Name = "MARGARITA CARRERA FOURNIER",
                    }
                    }
                },
            };

            foreach (var item in clases)
            {
                mp.Run(item.Value);
            }

            ExcelExport.Export(clases);
        }
    }
}