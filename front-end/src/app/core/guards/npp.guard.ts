import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { from, map, of, switchMap } from 'rxjs';
import { AuthServiceService } from '../../pages/auth/auth-services/auth-service.service';
import { NppModalService } from '../services/npp-modal.service';
import { NppService } from '../services/npp.service';

export const nppGuard: CanActivateFn = () => {
  const authService = inject(AuthServiceService);
  const nppService = inject(NppService);
  const nppModalService = inject(NppModalService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return nppService.checkStatus().pipe(
    switchMap((status) => {
      if (status.acknowledged) {
        return of(true);
      }

      return nppService.getContent().pipe(
        switchMap((content) => from(nppModalService.show(content))),
        switchMap((acknowledged) => {
          if (!acknowledged) {
            router.navigate(['/pages/home']);
            return of(false);
          }

          return nppService.acknowledge().pipe(map(() => true));
        })
      );
    })
  );
};
