import { Injectable } from '@angular/core';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Injectable({
  providedIn: 'root'
})
export class GridExportService {

  /**
   * Export to Excel
   * @param data Filtered grid data
   * @param fileName Output file name
   */
  exportToExcel(data: any[], fileName: string = 'iron_logic_export') {
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(workbook, worksheet, 'report');
    XLSX.writeFile(workbook, `${fileName}.xlsx`);
  }

  /**
   * Export to CSV
   * @param data Filtered grid data
   * @param fileName Output file name
   */
  exportToCsv(data: any[], fileName: string = 'iron_logic_export') {
    const worksheet = XLSX.utils.json_to_sheet(data);
    const csvOutput = XLSX.utils.sheet_to_csv(worksheet);

    const blob = new Blob([csvOutput], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');

    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `${fileName}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  /**
   * Export to PDF with Persian font support
   * @param data Filtered data
   * @param columnTitles Column titles (can be Persian)
   * @param columnFields Field names in data
   * @param fileName File name
   */
  exportToPdf(data: any[], columnTitles: string[], columnFields: string[], fileName: string = 'iron_logic_export') {
    const doc = new jsPDF('p', 'mm', 'a4');

    // Register Persian font (replace Base64 string here)
    // This method allows Persian characters to display correctly
    this.registerPersianFont(doc);

    const rows = data.map(item => columnFields.map(field => item[field] || ''));

    autoTable(doc, {
      head: [columnTitles],
      body: rows,
      theme: 'grid',
      styles: {
        font: 'Vazir', // Registered font name
        fontSize: 9,
        cellPadding: 3,
        valign: 'middle',
        halign: 'right', // Right-align for content
      },
      headStyles: {
        fillColor: [99, 102, 241],
        textColor: 255,
        fontStyle: 'bold',
        halign: 'center' // Center-align for header
      },
      // Set text direction to right-to-left (RTL)
      didDrawCell: (data) => {
        // Additional settings for RTL display if needed
      }
    });

    doc.save(`${fileName}.pdf`);
  }

  /**
   * Register Persian font in jsPDF
   * Note: Replace 'YOUR_BASE64_STRING_HERE' with your font's Base64 string
   */
  private registerPersianFont(doc: jsPDF) {
    // Example: Vazir font
    const vazirBase64 = 'YOUR_BASE64_STRING_HERE';

    if (vazirBase64 !== 'YOUR_BASE64_STRING_HERE') {
      doc.addFileToVFS('Vazir.ttf', vazirBase64);
      doc.addFont('Vazir.ttf', 'Vazir', 'normal');
      doc.setFont('Vazir');
    }
  }
}
