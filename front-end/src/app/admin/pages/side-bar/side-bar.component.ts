import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DoctorMENU, MENU } from '../../menu';

@Component({
  standalone: false,
  selector: 'app-side-bar',
  templateUrl: './side-bar.component.html'
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
