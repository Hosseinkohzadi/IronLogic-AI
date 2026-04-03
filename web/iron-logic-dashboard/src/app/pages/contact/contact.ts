import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule,RouterLink],
  templateUrl: './contact.html',
})
export class ContactComponent {}
