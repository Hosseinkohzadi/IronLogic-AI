import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink], // 🚀 این بخش حیاتی است
  templateUrl: './footer.html'
})
export class FooterComponent {}
