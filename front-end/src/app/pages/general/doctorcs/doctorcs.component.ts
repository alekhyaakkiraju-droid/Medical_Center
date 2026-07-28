import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { DoctorService } from '../services/doctor.service';
import { Doctor } from '../../models';
import { Subscription } from 'rxjs';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

@Component({
    selector: 'app-doctorcs',
    templateUrl: './doctorcs.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./doctorcs.component.css'],
    imports: [AssetUrlPipe]
})
export class DoctorcsComponent implements OnInit, OnDestroy {
  
  doctorsData: Doctor[] = [];
  private doctorSubscription!: Subscription;

  constructor(private doctorService: DoctorService) { }

  ngOnInit() {
    this.loadDoctor();
  }
  ngOnDestroy() {
    if (this.doctorSubscription) {
      this.doctorSubscription.unsubscribe();
    }
  }
  loadDoctor() {
    this.doctorSubscription = this.doctorService.getAllDoctors().subscribe(
      (result) => {
        if (result?.items) {
          this.doctorsData = result.items;
        } else {
        }
      },
      (error) => {
      }
    );
  }

 
}
