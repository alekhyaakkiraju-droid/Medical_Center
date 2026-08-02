import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { SpecializationService } from '../services/specialization.service';
import { Subscription } from 'rxjs';
import { DoctorcsComponent } from '../doctorcs/doctorcs.component';
import { FaqComponent } from '../faq/faq.component';
import { AppointmentRequestComponent } from '../appointment-request/appointment-request.component';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-Home',
    templateUrl: './Home.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./Home.component.css'],
    imports: [DoctorcsComponent, FaqComponent, AppointmentRequestComponent, AssetUrlPipe, RouterLink]
})
export class HomeComponent implements OnInit, OnDestroy {

  private subscriptions: Subscription[] = [];
  patients: any[] = [
    {
      name: 'Sarah Johnson',
      image: 'images/testimonials/1.jpg',
      reviews: [{ review: 'Excellent care and a smooth appointment experience.' }]
    },
    {
      name: 'Michael Chen',
      image: 'images/testimonials/2.jpg',
      reviews: [{ review: 'Professional staff and modern facilities.' }]
    }
  ];
  specializations: any[] = [];

  constructor(private specializationService: SpecializationService) {}

  ngOnInit(): void {
    this.subscriptions.push(
      this.specializationService.getSpecializations().subscribe({
        next: (data) => {
          this.specializations = data.items.slice(0, 6);
        },
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }


  services = [
    {
      image: "images/services/service-one.jpg",
      title: "Orthopedics",
      description:
        "Expert care for bone, joint, and muscle conditions. From fractures to joint replacements, we ensure mobility and pain relief with advanced orthopedic treatments.",
      link: "#",
    },
    {
      image: "images/services/service-two.jpg",
      title: "Diagnostic Services",
      description:
        "State-of-the-art imaging and laboratory tests for accurate disease detection. Our diagnostic services include MRI, CT scans, blood tests, and more.",
      link: "#",
    },
    {
      image: "images/services/service-three.jpg",
      title: "Psychology",
      description:
        "Comprehensive mental health support for stress, anxiety, and emotional well-being. Our psychologists provide therapy and counseling tailored to your needs.",
      link: "#",
    },
    {
      image: "images/services/service-four.jpg",
      title: "General Treatment",
      description:
        "Comprehensive primary care for all ages. From routine check-ups to common illnesses, our general practitioners provide expert medical care with a personal touch.",
      link: "#",
    },
  ];
  
}
