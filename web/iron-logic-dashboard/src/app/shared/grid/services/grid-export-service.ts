import { Injectable } from '@angular/core';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Injectable({
  providedIn: 'root'
})
export class GridExportService {

  /**
   * خروجی اکسل (Excel)
   * @param data داده‌های فیلتر شده گرید
   * @param fileName نام فایل خروجی
   */
  exportToExcel(data: any[], fileName: string = 'iron_logic_export') {
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(workbook, worksheet, 'گزارش');
    XLSX.writeFile(workbook, `${fileName}.xlsx`);
  }

  /**
   * خروجی CSV
   * @param data داده‌های فیلتر شده گرید
   * @param fileName نام فایل خروجی
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
   * خروجی PDF با پشتیبانی از فونت فارسی
   * @param data داده‌های فیلتر شده
   * @param columnTitles عناوین ستون‌ها (فارسی)
   * @param columnFields نام فیلدها در دیتا
   * @param fileName نام فایل
   */
  exportToPdf(data: any[], columnTitles: string[], columnFields: string[], fileName: string = 'iron_logic_export') {
    const doc = new jsPDF('p', 'mm', 'a4');

    // ثبت فونت فارسی (در اینجا باید رشته Base64 فونت را جایگزین کنید)
    // این متد اجازه می‌دهد حروف فارسی به درستی نمایش داده شوند
    this.registerPersianFont(doc);

    const rows = data.map(item => columnFields.map(field => item[field] || ''));

    autoTable(doc, {
      head: [columnTitles],
      body: rows,
      theme: 'grid',
      styles: {
        font: 'Vazir', // نام فونت ثبت شده
        fontSize: 9,
        cellPadding: 3,
        valign: 'middle',
        halign: 'right', // راست‌چین برای محتوا
      },
      headStyles: {
        fillColor: [99, 102, 241],
        textColor: 255,
        fontStyle: 'bold',
        halign: 'center' // مرکز‌چین برای هدر
      },
      // تنظیم جهت متن به راست‌چین (RTL)
      didDrawCell: (data) => {
        // تنظیمات اضافی برای بهبود نمایش RTL در صورت نیاز
      }
    });

    doc.save(`${fileName}.pdf`);
  }

  /**
   * ثبت فونت فارسی در jsPDF
   * نکته: شما باید رشته Base64 فونت خود را در قسمت 'YOUR_BASE64_STRING_HERE' قرار دهید
   */
  private registerPersianFont(doc: jsPDF) {
    // برای مثال فونت Vazir
    const vazirBase64 = 'YOUR_BASE64_STRING_HERE';

    if (vazirBase64 !== 'YOUR_BASE64_STRING_HERE') {
      doc.addFileToVFS('Vazir.ttf', vazirBase64);
      doc.addFont('Vazir.ttf', 'Vazir', 'normal');
      doc.setFont('Vazir');
    }
  }
}
