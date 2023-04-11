using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Drawing;
using Syncfusion.Pdf.Grid;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using Syncfusion.Pdf.Interactive;
using EssentialPDFSamples;

namespace ZUGFeRDSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            #region Fields
            //Create border color
            PdfColor borderColor = new PdfColor(Color.FromArgb(255, 142, 170, 219));
            PdfBrush lightBlueBrush = new PdfSolidBrush(new PdfColor(Color.FromArgb(255, 91, 126, 215)));

            PdfBrush darkBlueBrush = new PdfSolidBrush(new PdfColor(Color.FromArgb(255, 65, 104, 209)));

            PdfBrush whiteBrush = new PdfSolidBrush(new PdfColor(Color.FromArgb(255, 255, 255, 255)));
            PdfPen borderPen = new PdfPen(borderColor, 1f);

            //Create TrueType font
            PdfTrueTypeFont headerFont = new PdfTrueTypeFont(new Font("Arial", 30, System.Drawing.FontStyle.Regular), true);
            PdfTrueTypeFont arialRegularFont = new PdfTrueTypeFont(new Font("Arial", 9, System.Drawing.FontStyle.Regular), true);
            PdfTrueTypeFont arialBoldFont = new PdfTrueTypeFont(new Font("Arial", 11, System.Drawing.FontStyle.Bold), true);


            const float margin = 30;
            const float lineSpace = 7;
            const float headerHeight = 90;
            #endregion

            #region header and buyer infomation
            //Create PDF with PDF/A-3b conformance
            PdfDocument document = new PdfDocument(PdfConformanceLevel.Pdf_A3B);
            //Set ZUGFeRD profile
            document.ZugferdConformanceLevel = ZugferdConformanceLevel.Basic;

            //Add page to the PDF
            PdfPage page = document.Pages.Add();

            PdfGraphics graphics = page.Graphics;
           
            //Get the page width and height
            float pageWidth = page.GetClientSize().Width;
            float pageHeight = page.GetClientSize().Height;
            //Draw page border
            graphics.DrawRectangle(borderPen, new RectangleF(0, 0, pageWidth, pageHeight));

            //Fill the header with light Brush 
            graphics.DrawRectangle(lightBlueBrush, new RectangleF(0, 0, pageWidth, headerHeight));

            RectangleF headerAmountBounds = new RectangleF(400, 0, pageWidth - 400, headerHeight);           

            graphics.DrawString("INVOICE", headerFont, whiteBrush, new PointF(margin, headerAmountBounds.Height / 3));

            graphics.DrawRectangle(darkBlueBrush, headerAmountBounds);

            graphics.DrawString("Amount", arialRegularFont, whiteBrush, headerAmountBounds, new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle));

            PdfTextElement textElement = new PdfTextElement("Invoice Number: 2058557939", arialRegularFont);
                        
            PdfLayoutResult layoutResult = textElement.Draw(page, new PointF(headerAmountBounds.X - margin, 120));

            textElement.Text = "Date : " + DateTime.Now.ToString("dddd dd, MMMM yyyy");
            textElement.Draw(page, new PointF(layoutResult.Bounds.X, layoutResult.Bounds.Bottom + lineSpace));

            textElement.Text = "Bill To:";
            layoutResult = textElement.Draw(page, new PointF(margin, 120));

            textElement.Text = "Abraham Swearegin,";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            textElement.Text = "United States, California, San Mateo,";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            textElement.Text = "9920 BridgePointe Parkway,";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            textElement.Text = "9365550136";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            #endregion

            #region Invoice data
            PdfGrid grid = new PdfGrid();

            grid.DataSource = GetProductReport();

            grid.Columns[1].Width = 150;
            grid.Style.Font = arialRegularFont;
            grid.Style.CellPadding.All = 5;

            grid.ApplyBuiltinStyle(PdfGridBuiltinStyle.ListTable4Accent5);

            layoutResult = grid.Draw(page, new PointF(0, layoutResult.Bounds.Bottom + 40));

            textElement.Text = "Grand Total: ";
            textElement.Font = arialBoldFont;
            layoutResult = textElement.Draw(page, new PointF(headerAmountBounds.X - 40, layoutResult.Bounds.Bottom + lineSpace));
            
            float totalAmount = GetTotalAmount(grid);
            textElement.Text = "$" + totalAmount.ToString();
            layoutResult = textElement.Draw(page, new PointF(layoutResult.Bounds.Right + 4, layoutResult.Bounds.Y));

            graphics.DrawString("$" + totalAmount.ToString(), arialBoldFont, whiteBrush, new RectangleF(400, lineSpace, pageWidth - 400, headerHeight + 15), new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle));
            #endregion

            #region Seller information
            borderPen.DashStyle = PdfDashStyle.Custom;
            borderPen.DashPattern = new float[] { 3, 3 };

            PdfLine line = new PdfLine(borderPen, new PointF(0, 0), new PointF(pageWidth, 0));
            layoutResult = line.Draw(page, new PointF(0, pageHeight - 100));

            textElement.Text = "800 Interchange Blvd.";
            textElement.Font = arialRegularFont;
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + (lineSpace * 3)));
            textElement.Text = "Suite 2501,  Austin, TX 78721";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            textElement.Text = "Any Questions? support@adventure-works.com";
            layoutResult = textElement.Draw(page, new PointF(margin, layoutResult.Bounds.Bottom + lineSpace));
            #endregion

            #region Create ZUGFeRD XML

            //Create //ZUGFeRD Invoice
            ZugferdInvoice invoice = new ZugferdInvoice("2058557939", DateTime.Now, CurrencyCodes.USD);

            //Set ZUGFeRD profile to basic
            invoice.Profile = ZugferdProfile.Basic;

            //Add buyer details
            invoice.Buyer = new UserDetails
            {
                ID = "Abraham_12",
                Name = "Abraham Swearegin",
                ContactName = "Swearegin",
                City = "United States, California",
                Postcode = "9920",
                Country = CountryCodes.US,
                Street = "9920 BridgePointe Parkway"
            };

            //Add seller details
            invoice.Seller = new UserDetails
            {
                ID = "Adventure_123",
                Name = "AdventureWorks",
                ContactName = "Adventure support",
                City = "Austin,TX",
                Postcode = "78721",
                Country = CountryCodes.US,
                Street = "800 Interchange Blvd"
            };


            IEnumerable<Product> products = GetProductReport();

            foreach (Product product in products)
                invoice.AddProduct(product);


            invoice.TotalAmount = totalAmount;

            MemoryStream zugferdXML = new MemoryStream();
            invoice.Save(zugferdXML);
            #endregion

            #region Embed ZUGFeRD XML to PDF
            
            //Attach ZUGFeRD XML to PDF
            PdfAttachment attachment = new PdfAttachment("ZUGFeRD-invoice.xml", zugferdXML);
            attachment.Relationship = PdfAttachmentRelationship.Alternative;
            attachment.ModificationDate = DateTime.Now;
            attachment.Description = "ZUGFeRD-invoice";
            attachment.MimeType = "application/xml";
            document.Attachments.Add(attachment);
            #endregion

            document.Save("ZUGFeRDInvoice.pdf");
            document.Close(true);

            Process.Start("ZUGFeRDInvoice.pdf");
        }

 
        private IEnumerable<Product> GetProductReport()
        {
            
            //Read product data from the XML data source
            FileStream xmlStream = new FileStream( "../../Data/InvoiceProductList.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            xmlStream.Position = 0;
            using (StreamReader reader = new StreamReader(xmlStream, true))
            {
                return XElement.Parse(reader.ReadToEnd())
                    .Elements("ProductDetails")
                    .Select(c => new Product
                    {
                        ProductID = c.Element("Productid").Value,
                        ProductName = c.Element("Product").Value,
                        Price = float.Parse(c.Element("Price").Value),
                        Quantity = float.Parse(c.Element("Quantity").Value),
                        Total = float.Parse(c.Element("Total").Value)
                    });
            }
        }

        /// <summary>
        /// Get the total amount of purcharsed items
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private float GetTotalAmount(PdfGrid grid)
        {
            float Total = 0f;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                string cellValue = grid.Rows[i].Cells[grid.Columns.Count - 1].Value.ToString();
                float result;
                Total += float.TryParse(cellValue, out result) ? result : 0;
            }
            return Total;

        }
    }
    
}
