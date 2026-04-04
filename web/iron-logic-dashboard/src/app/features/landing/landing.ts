import {Component} from '@angular/core';
import {RouterLink} from '@angular/router';
import {Testimonials} from '@features/testimonials/testimonials';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, Testimonials],
  templateUrl: './landing.html',
})
export class LandingComponent {

}
