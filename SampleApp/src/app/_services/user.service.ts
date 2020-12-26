import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { UserParams } from '../_models/userParams';
import { AuthService } from './auth.service';


@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;
  memberCache = new Map();

  constructor(private http: HttpClient) { }

  getUsers(userParams: UserParams){
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if(response)
    {
      return of(response);
    }
    
    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    
    return this.getPaginatedResult<User[]>(this.baseUrl+'users', params)
    .pipe(map(response=>{
      this.memberCache.set(Object.values(userParams).join('-'), response);
      return response;
    }));
  }

  getUser(id): Observable<User>{
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((user: User) => user.id === id);

    if(member)
    {
      return of(member);
    }
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User){
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, id: number){
    return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain', {});
  }
  deletePhoto(userId: number, id: number){
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number){
    let params = new HttpParams();
    
    params=params.append('pageNumber',pageNumber.toString());
    params=params.append('pageSize',pageSize.toString());

    return params;
  }
}
