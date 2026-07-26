import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { DoctorMENU, MENU } from '../../menu';
import { LogoutComponent } from '../../../pages/auth/logout/logout.component';

@Component({
    selector: 'app-side-bar',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './side-bar.component.html',
    imports: [LogoutComponent]
})
export class SideBarComponent implements OnInit {
  menuItems: any;
  isDropdownOpen = false;

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.checkIfDoctorRoute();
  }

  checkIfDoctorRoute(): void {
    if (this.router.url.includes('doctor/')) {
      this.menuItems = DoctorMENU;
    } else {
      this.menuItems = MENU;
    }
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }
}
