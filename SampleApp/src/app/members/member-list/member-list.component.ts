import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { AlertifyService } from '../../_services/Alertify.service';
import { UserService } from '../../_services/user.service';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: 'Male'}, {value: 'female', display: 'Female'}]

  constructor(private userService: UserService, private alertify: AlertifyService) { 
              }

  ngOnInit() {
    var userId = JSON.parse(localStorage.getItem('user')).id;
    this.userService.getUser(userId).subscribe(value => {
      this.user = value;
      this.userParams = new UserParams(this.user);
      this.loadUsers();
    });
  }

  loadUsers(){
    this.userService.getUsers(this.userParams).subscribe(users => {
      this.users = users.result;
      this.pagination = users.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }

  pageChanged(event: any){
    this.userParams.pageNumber = event.page;
    this.loadUsers();
  }

  resetFilters(){
    this.userParams = new UserParams(this.user);
    this.loadUsers();
  }
}
