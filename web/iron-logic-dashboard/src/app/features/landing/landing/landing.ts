import { Component } from '@angular/core';
import { RouterLink } from '@angular/router'; // ۱. این خط اضافه شد

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink], // ۲. این کلمه اضافه شد
  templateUrl: './landing.html',
})
export class LandingComponent {

}
