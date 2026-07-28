import { RouterLink } from '@angular/router';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-services/auth-service.service';
import { getRoleBasedRedirectUrl } from '../../../core/utils/role-redirect.util';

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
        this.router.navigate([getRoleBasedRedirectUrl(user.roles)]);
      } else {
        this.router.navigate(['/auth/login']);
      }
    });
  }
}
