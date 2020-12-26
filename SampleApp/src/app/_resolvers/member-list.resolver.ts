import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/Alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Pagination } from '../_models/pagination';
import { UserParams } from '../_models/userParams';

@Injectable()

export class MemberListResolver implements Resolve<User[]> {
    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService){}
    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        let userParams: UserParams;
        return this.userService.getUsers(userParams).pipe(
            catchError(error => {
                this.alertify.error('Problem retreiving data...');
                this.router.navigate(['/home']);
                return of(null);
            })
        );
    }
}


