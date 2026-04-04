import {Component} from '@angular/core';

@Component({
  selector: 'app-testimonials',
  imports: [],
  templateUrl: './testimonials.html',
  styleUrl: './testimonials.css',
})
export class Testimonials {
  // testimonials.component.ts
  testimonials = [
    {
      name: 'SHANINA SHAIK',
      image: 'assets/loginImage.jpg',
      text: "I have worked with IronLogic AI for a few years now. I absolutely love the energy and how motivating the AI Coach is.",
      stars: 5
    },
    {
      name: 'ALEX RIVERA',
      image: 'assets/Review-1@2x-550x726.webp',
      text: "The personalized programs changed my life. I feel empowered, confident and healthy.",
      stars: 5
    },
    {
      name: 'JORDAN KNIGHT',
      image: 'assets/Review-2@2x-550x726.webp',
      text: "Best investment for my fitness journey. The AI insights are incredibly accurate.",
      stars: 4
    },
    {
      name: 'JORDAN KNIGHT',
      image: 'assets/Review-3@2x-550x726.webp',
      text: "Working with my personal trainer completely changed my fitness journey. I’ve gained strength, improved posture, and feel more confident than ever!",
      stars: 5
    }
  ];

  currentIndex = 0;

  getStars(count: number): number[] {
    return Array(count).fill(0);
  }

  nextSlide() {
    this.currentIndex = (this.currentIndex + 1) % this.testimonials.length;
  }

  prevSlide() {
    this.currentIndex = (this.currentIndex - 1 + this.testimonials.length) % this.testimonials.length;
  }
}
