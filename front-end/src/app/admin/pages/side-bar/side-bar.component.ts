import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DoctorMENU, MENU } from '../../menu';
import { LogoutComponent } from '../../../pages/auth/logout/logout.component';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';

@Component({
    selector: 'app-side-bar',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './side-bar.component.html',
    imports: [LogoutComponent, RouterLink, RouterLinkActive]
})
export class SideBarComponent implements OnInit {
  menuItems: typeof MENU = MENU;
  userLabel = 'Administrator';

  constructor(
    private router: Router,
    private authService: AuthServiceService
  ) {}

  ngOnInit(): void {
    this.checkIfDoctorRoute();
    this.userLabel = this.authService.getUserName() ?? this.authService.getUsernameFromToken() ?? 'Administrator';
  }

  checkIfDoctorRoute(): void {
    if (this.router.url.includes('doctor/')) {
      this.menuItems = DoctorMENU;
    } else {
      this.menuItems = MENU;
    }
  }

  isModalItem(item: (typeof MENU)[number]): boolean {
    return item.toggle === 'modal';
  }
}
