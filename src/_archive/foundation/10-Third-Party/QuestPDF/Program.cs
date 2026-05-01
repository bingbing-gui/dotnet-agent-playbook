using QuestPDF.Infrastructure;
using QuestPDF;
using QuestPDF.Fluent;

namespace QuestPDF.Library
{
    class Program
    {
        static void Main(string[] args)
        {

            Settings.License = LicenseType.Community;

 
            var model = InvoiceDocumentDataSource.GetInvoiceDetails();
            var document = new InvoiceDocument(model);

            document.GeneratePdfAndShow();


        }
    }
}