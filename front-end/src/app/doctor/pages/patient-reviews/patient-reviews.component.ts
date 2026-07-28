import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ReloadService } from '../../../shared/service/reload.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { RelatedPatientsReviewsService } from '../../services/related-patients-reviews.service';
import { Subscription } from 'rxjs';
import { SideBarComponent } from '../../../admin/pages/side-bar/side-bar.component';

@Component({
    selector: 'app-patient-reviews',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './patient-reviews.component.html',
    imports: [SideBarComponent]
})
export class PatientReviewsComponent implements OnInit, OnDestroy {

  patientReviewsSubscribtion !: Subscription;
  constructor(private reload: ReloadService, private patientsReviewService: RelatedPatientsReviewsService, private authService: AuthServiceService) { }
  ngOnDestroy(): void {
    if (this.patientReviewsSubscribtion) {
      this.patientReviewsSubscribtion.unsubscribe();
    }
  }

  ngOnInit() {
    this.setDoctorId();
    this.getPatientsReview();
  }
  ngAfterViewInit(): void {
    this.reload.initializeLoader();
  }

  doctorId: string = '';
  errorMessage: string = '';
  reviews: any[] = [];
  setDoctorId(): void {
    const id = this.authService.getNameIdentifier();
    if (id) {
      this.doctorId = id;
    } else {
    }
  }

  getPatientsReview(): void {
    if (this.doctorId == null) {
    }
    this.patientReviewsSubscribtion = this.patientsReviewService.getPatientsReview(this.doctorId).subscribe({
      next: (data) => {
        this.reviews = data.items ?? [];
      },
      error: (error) => {
      },
    });
  }
}
