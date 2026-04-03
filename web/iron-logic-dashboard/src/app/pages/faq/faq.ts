import {Component, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './faq.html'
})
export class FaqComponent {
  categories = [
    {id: 'Membership & Plans', icon: 'membership'},
    {id: 'Training & Facilities', icon: 'training'},
    {id: 'Billing & Policies', icon: 'billing'}
  ];
  activeCategory = signal('Membership & Plans');
  openIndex = signal<number | null>(0);

  faqs = [
    // --- Membership & Plans ---
    {
      category: 'Membership & Plans',
      question: 'What types of memberships do you offer?',
      answer: 'We offer monthly, quarterly, and annual memberships, as well as personal training and family packages to suit your fitness goals.'
    },
    {
      category: 'Membership & Plans',
      question: 'How can I join the gym?',
      answer: 'Simply visit our front desk or sign up online. Our team will guide you through choosing the right membership plan for your needs.'
    },
    {
      category: 'Membership & Plans',
      question: 'Do you offer free trials?',
      answer: 'Yes! We provide a one-day free trial so you can experience our facilities and classes before committing to a membership.'
    },
    {
      category: 'Membership & Plans',
      question: 'Can I freeze or pause my membership?',
      answer: 'Memberships can be paused for travel or medical reasons. Contact our front desk for details and documentation requirements.'
    },
    {
      category: 'Membership & Plans',
      question: 'Do you have discounts or referral bonuses?',
      answer: 'Yes! Members who refer friends receive special rewards, and we often run seasonal promotions for new joiners.'
    },

    // --- Training & Facilities ---
    {
      category: 'Training & Facilities',
      question: 'What kind of equipment do you have?',
      answer: 'Our gym features state-of-the-art cardio machines, free weights, resistance equipment, functional training zones, and a yoga studio.'
    },
    {
      category: 'Training & Facilities',
      question: 'Do you offer personal training?',
      answer: 'Yes, certified personal trainers are available for one-on-one or small group sessions to help you reach your fitness goals faster.'
    },
    {
      category: 'Training & Facilities',
      question: 'What classes are included with membership?',
      answer: 'All memberships include access to yoga, HIIT, spinning, Zumba, pilates, and strength classes—scheduled daily by our instructors.'
    },
    {
      category: 'Training & Facilities',
      question: 'Do you have locker rooms and showers?',
      answer: 'Yes, we provide clean locker rooms, private showers, and secure storage for all members.'
    },
    {
      category: 'Training & Facilities',
      question: 'Are group classes beginner-friendly?',
      answer: 'Absolutely! All group classes are designed for all levels, and instructors provide modifications for beginners and advanced members alike.'
    },

    // --- Billing & Policies ---
    {
      category: 'Billing & Policies',
      question: 'What payment methods do you accept?',
      answer: 'We accept debit/credit cards, bank transfers, and digital wallets. Payments can be made in person or through our online portal.'
    },
    {
      category: 'Billing & Policies',
      question: 'Can I get an invoice or receipt for my membership?',
      answer: 'Yes, every transaction includes a digital receipt, and you can request a full invoice from our front desk or via email.'
    },
    {
      category: 'Billing & Policies',
      question: 'What is your cancellation policy?',
      answer: 'Memberships can be canceled anytime with 7 days’ notice. Refunds depend on the plan type and remaining duration.'
    },
    {
      category: 'Billing & Policies',
      question: 'Are there any additional fees?',
      answer: 'No hidden fees. Only optional add-ons such as personal training or nutrition consultations have extra charges.'
    },
    {
      category: 'Billing & Policies',
      question: 'How do I update my billing information?',
      answer: 'You can update your payment details anytime at the front desk or through your member dashboard online.'
    }
  ];

  toggleFaq(index: number) {
    this.openIndex.set(this.openIndex() === index ? null : index);
  }
}
