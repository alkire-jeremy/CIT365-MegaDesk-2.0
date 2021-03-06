﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

/* DeskQuote.cs (Class): This class will define the attributes of a quote 
 * including Desk, rush days, customer name, and quote date. This class will 
 * also hold the logic in determining the desk quote total.*/
namespace MegaDesk2
{
    class DeskQuote
    {
        public SurfaceMaterial DeskMaterial { get; set; }
        public int ShippingDays { get; set; } // 3, 5, or 7 days. Normal production time = 14 days.
        public string CustomerName { get; set; }
        public DateTime QuoteDate { get; set; }
        public double QuoteTotal { get; set; }
        public int Width { get; set; }
        public int Depth { get; set; }
        public int NumberOfDrawers { get; set; }
        public Desk Desk { get; set; }

        public const int SQUAREPRICE = 1;
        public const int DRAWERPRICE = 50;
        public List<DeskQuote> deskQuotes = new List<DeskQuote>();
        public void writeQuoteToFile(string customerName, Desk desk, int width, int depth,
            SurfaceMaterial material, int numberOfDrawers, int rushOrderOptions)
        {
            DeskQuote deskQuote = new DeskQuote();
            deskQuote.CustomerName = customerName;
            deskQuote.Desk = desk;
            deskQuote.Desk.Width = width;
            deskQuote.Desk.Depth = depth;
            deskQuote.Desk.DeskMaterial = material;
            deskQuote.Desk.NumberOfDrawers = numberOfDrawers;
            deskQuote.ShippingDays = rushOrderOptions;
            deskQuote.QuoteTotal = calculateTotalQuote(deskQuote.Desk, deskQuote.Desk.Width,
            deskQuote.Desk.Depth, deskQuote.Desk.DeskMaterial, deskQuote.Desk.NumberOfDrawers, deskQuote.ShippingDays);
            deskQuote.QuoteDate = DateTime.Now;
            deskQuote.DeskMaterial = material;
            deskQuote.NumberOfDrawers = numberOfDrawers;
            deskQuote.Depth = depth;
            deskQuote.Width = width;

            string date = deskQuote.QuoteDate.ToString("MM/dd/yyyy");


            string displayOutput = "Customer Name: " + deskQuote.CustomerName + Environment.NewLine +
                            "Desk Width: " + deskQuote.Desk.Width + Environment.NewLine +
                            "Desk Depth: " + deskQuote.Desk.Depth + Environment.NewLine +
                            "Desk Material: " + deskQuote.Desk.DeskMaterial + Environment.NewLine +
                            "Desk Drawer Count: " + deskQuote.Desk.NumberOfDrawers + Environment.NewLine +
                            "Rush Order: " + deskQuote.ShippingDays + Environment.NewLine +
                            "Quate Date: " + date + Environment.NewLine +
                            "Quote Total: $" + deskQuote.QuoteTotal;
            DisplayQuote.Quote = displayOutput;

            if (!File.Exists("..\\..\\quotes.json"))
            {
                File.Create("..\\..\\quotes.json").Close();
            }

            var data = File.ReadAllText("..\\..\\quotes.json");

            var listQuotes = JsonConvert.DeserializeObject<List<DeskQuote>>(data) ?? new List<DeskQuote>();

            listQuotes.Add(deskQuote);

            data = JsonConvert.SerializeObject(listQuotes.ToArray());
            File.WriteAllText("..\\..\\quotes.json", data);

        }

        public double calculateTotalQuote(Desk desk, int width, int depth,
            SurfaceMaterial material, int numberOfDrawers, int rushOrderOptions)
        {
            double rushPrice = 0;
            double materialPrice = 0;
            double surfaceArea = width * depth;
            double[,] rushOrderPriceMap = GetRushOrder(surfaceArea, rushOrderOptions);
            List<int> materialPriceList = new List<int>() { 200, 100, 50, 300, 125 };

            // Calculate materialPrice.
            if (material == SurfaceMaterial.Oak)
            {
                materialPrice = materialPriceList[0];
            }
            else if (material == SurfaceMaterial.Laminate)
            {
                materialPrice = materialPriceList[1];
            }
            else if (material == SurfaceMaterial.Pine)
            {
                materialPrice = materialPriceList[2];
            }
            else if (material == SurfaceMaterial.Rosewood)
            {
                materialPrice = materialPriceList[3];
            }
            else if (material == SurfaceMaterial.Veneer)
            {
                materialPrice = materialPriceList[4];
            }

            // Calculate rushPrice.
            switch (rushOrderOptions)
            {
                case 3:
                    if (surfaceArea < 1000)
                    {
                        rushPrice = rushOrderPriceMap[0, 0];
                    }
                    else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                    {
                        rushPrice = rushOrderPriceMap[0, 1];
                    }
                    else if (surfaceArea > 2000)
                    {
                        rushPrice = rushOrderPriceMap[0, 2];
                    }
                    break;
                case 5:
                    if (surfaceArea < 1000)
                    {
                        rushPrice = rushOrderPriceMap[1, 0];
                    }
                    else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                    {
                        rushPrice = rushOrderPriceMap[1, 1];
                    }
                    else if (surfaceArea > 2000)
                    {
                        rushPrice = rushOrderPriceMap[1, 2];
                    }
                    break;
                case 7:
                    if (surfaceArea < 1000)
                    {
                        rushPrice = rushOrderPriceMap[2, 0];
                    }
                    else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                    {
                        rushPrice = rushOrderPriceMap[2, 1];
                    }
                    else if (surfaceArea > 2000)
                    {
                        rushPrice = rushOrderPriceMap[2, 2];
                    }
                    break;
                default:
                    break;
            }

            // Get total price of order
            if (surfaceArea > 1000)
            {
                double total = (surfaceArea * SQUAREPRICE) + (numberOfDrawers * DRAWERPRICE) + rushPrice + materialPrice + 200;
                return total;
            }
            else
            {
                double total = (numberOfDrawers * DRAWERPRICE) + rushPrice + materialPrice + 200;
                return total;
            }
        }

        private double[,] GetRushOrder(double surfaceArea, int rushOrderOptions)
        {
            // Read file as a list of strings, assign it to rushPriceStrings[]
            string[] rushPriceStrings = File.ReadAllLines("..\\..\\Resources\\rushOrderPrices.txt");

            // Create list of doubles to hold rushPrices as a list.
            List<double> rushPricesList = new List<double>();

            for (int i = 0; i < rushPriceStrings.Length; i++)
            {
                // Add rushPriceStrings[] elements to rushPricesList.
                rushPricesList.Add(Convert.ToDouble(rushPriceStrings[i]));
            }

            double[,] rushPriceMap = new double[3, 3];

            try
            {
                // Add the prices to the 2D pricing map
                foreach (double price in rushPricesList)
                {
                    if (rushOrderOptions == 3)
                    {
                        if (surfaceArea < 1000)
                            rushPriceMap[0, 0] = price;
                        else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                            rushPriceMap[0, 1] = price;
                        else
                            rushPriceMap[0, 2] = price;
                    }
                    else if (rushOrderOptions == 5)
                    {
                        if (surfaceArea < 1000)
                            rushPriceMap[1, 0] = price;
                        else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                            rushPriceMap[1, 1] = price;
                        else
                            rushPriceMap[1, 2] = price;
                    }
                    else if (rushOrderOptions == 7)
                    {
                        if (surfaceArea < 1000)
                            rushPriceMap[2, 0] = price;
                        else if (surfaceArea >= 1000 && surfaceArea <= 2000)
                            rushPriceMap[2, 1] = price;
                        else
                            rushPriceMap[2, 2] = price;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error occurred while populating Rush Order data.", "Error");
            }

            return rushPriceMap;
        }
    }
}
