import { AfterViewInit, Component, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { ReloadService } from './shared/service/reload.service';
import { NgIf } from '@angular/common';
import { HeaderComponent } from './layout/header/header.component';
import { FooterComponent } from './layout/footer/footer.component';
import { SkipToContentComponent } from './shared/components/skip-to-content/skip-to-content.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [NgIf, HeaderComponent, FooterComponent, RouterOutlet, SkipToContentComponent]
})
export class AppComponent implements OnInit, AfterViewInit {
  title = 'MedicalCenter';
  showHeaderAndNavbar = true;
  constructor(private router: Router, private reload: ReloadService) {}
  ngAfterViewInit(): void { this.hideRoutePreloader(); }
  ngOnInit(): void {
    this.updateChromeVisibility(this.router.url);
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.updateChromeVisibility(event.urlAfterRedirects);
        setTimeout(() => this.hideRoutePreloader(), 0);
      });
  }
  private updateChromeVisibility(url: string): void {
    this.showHeaderAndNavbar = !url.includes('/admin') && !url.includes('/doctor') && !url.includes('/error') && !url.includes('/auth');
  }
  private hideRoutePreloader(): void { this.reload.initializeLoader(); }
}
