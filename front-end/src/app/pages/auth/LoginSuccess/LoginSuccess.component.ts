import { RouterLink } from '@angular/router';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-services/auth-service.service';

@Component({
    selector: 'app-LoginSuccess',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './LoginSuccess.component.html',
    imports: [RouterLink],
})
export class LoginSuccessComponent implements OnInit {

  constructor(
    private router: Router,
    private authService: AuthServiceService,
  ) { }

  ngOnInit(): void {
    this.authService.loadCurrentUser().subscribe((user) => {
      if (user) {
        if (this.authService.isRole('admin')) {
          this.router.navigate(['admin/dashboard']);
        } else if (this.authService.isRole('doctor')) {
          this.router.navigate(['doctor/doctor-appointments']);
        } else {
          this.router.navigate(['/pages/home']);
        }
      } else {
        this.router.navigate(['/auth/login']);
      }
    });
  }
}
