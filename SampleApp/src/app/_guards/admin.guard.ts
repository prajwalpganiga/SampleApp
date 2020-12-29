import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';
import { AlertifyService } from '../_services/Alertify.service';
import { AuthService } from '../_services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private alertifyService: AlertifyService
  ) {}
  canActivate(): Observable<boolean> {
    return this.authService.currentUser$.pipe(
      map((user: User) => {
        if (user.roles.includes('Admin') || user.roles.includes('Moderator')) {
          return true;
        }
        this.alertifyService.error("You can't enter this area");
      })
    );
  }
}
