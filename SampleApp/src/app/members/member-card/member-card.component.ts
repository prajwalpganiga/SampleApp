import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AlertifyService } from 'src/app/_services/Alertify.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { UserService } from 'src/app/_services/user.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() user: User;
  @Input() showLike: boolean = true;

  constructor(private UserService: UserService, private alertifyService: AlertifyService,
    public presence: PresenceService) { }

  ngOnInit() {
  }

  addLike(user: User){
    this.UserService.addLike(user.username).subscribe(() => {
      this.alertifyService.success("You have liked " + user.knownAs);
    });
  }

}
