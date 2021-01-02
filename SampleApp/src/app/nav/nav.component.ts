import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/Alertify.service';
import { Router } from '@angular/router';
import { PresenceService } from '../_services/presence.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

  constructor(public authService: AuthService, 
    private alertify: AlertifyService, 
    private router: Router,
    private presenceService: PresenceService) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  login()
  {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('Logged in Succesfuly');
    }, error => {
      this.alertify.error('Failed to Login');
    }, () => {
      this.router.navigate(['/members']);
    });
  }

  loggedIn(){
    return this.authService.loggedIn();
  }

  logout(){
    this.presenceService.stopHubConnection();
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodedToken = null;
    this.authService.currentUser = null;
    this.authService.currentUserSource.next(null);
    this.alertify.message('Logged Out');
    this.router.navigate(['/home']);
  }

}
