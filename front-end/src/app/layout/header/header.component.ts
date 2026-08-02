import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Subscription } from 'rxjs';
import { LogoutComponent } from '../../pages/auth/logout/logout.component';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./header.component.css'],
    imports: [RouterLink, RouterLinkActive, LogoutComponent]
})
export class HeaderComponent implements OnInit , OnDestroy {

  loggedStatusSubscription!: Subscription;
  constructor(private authService:AuthServiceService,
              private router : Router,
  ) { }
  ngOnDestroy(): void {
   if (this.loggedStatusSubscription) {
     this.loggedStatusSubscription.unsubscribe();
   }
  }


  isLoggedIn = false;
  authReady = false;
  
  ngOnInit() {
    this.loggedStatusSubscription = this.authService.getloggedStatus().subscribe(status => {
      this.isLoggedIn = status;
    });
    this.authService.getSessionReady().subscribe(ready => {
      this.authReady = ready;
    });
  }
  
  //---------------------Toggle----------------------

  isCollapsed = true;
  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed;
  }

  isDrawerOpen = false;
  toggleDrawer() {
    this.isDrawerOpen = !this.isDrawerOpen;
  } 


//-------------------- social icons loops-------------------
  ribbonLinks = [
    { href: 'mailto:support@careshift.com', icon: 'fa-solid fa-envelope', label: 'support@careshift.com', aria: 'Email support' },
    { href: 'tel:+15550100200', icon: 'fa-solid fa-phone', label: 'Emergency: +1 (555) 010-0200', aria: 'Emergency phone' },
    { href: 'https://facebook.com', icon: 'fa-brands fa-facebook-f', label: '', aria: 'Facebook' },
    { href: 'https://linkedin.com', icon: 'fa-brands fa-linkedin-in', label: '', aria: 'LinkedIn' },
    { href: 'https://instagram.com', icon: 'fa-brands fa-instagram', label: '', aria: 'Instagram' },
  ];
  
  menuItems = [
    { href: '/', label: 'Home' },
    { href: '/pages/about-us', label: 'About' },
    { href: '/pages/service', label: 'Service' },
    { href: '/pages/gallery', label: 'Gallery' },
    { href: '/pages/team', label: 'Team' },
    { href: '/pages/appointment', label: 'Appointment' },
    { href: '/pages/blog', label: 'Blog' },
    { href: '/pages/contact', label: 'Contact' }
  ];

}
