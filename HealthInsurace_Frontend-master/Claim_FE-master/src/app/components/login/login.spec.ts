import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ChangeDetectorRef, Component } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';
 
import { LoginComponent } from './login';
import { AuthService } from '../../services/auth.service';
 
@Component({
  selector: 'app-dummy',
  template: '',
  standalone: true
})
class DummyComponent {}
 
describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
 
  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login', 'storeTokens', 'getUserRole']);
 
    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        LoginComponent,
        DummyComponent
      ],
      providers: [
        { provide: AuthService, useValue: authSpy },
        ChangeDetectorRef,
        provideRouter([
          { path: 'admin/admin-dashboard', component: DummyComponent },
          { path: 'policyholder/policyholder-dashboard', component: DummyComponent },
          { path: 'manager', component: DummyComponent }
        ])
      ]
    }).compileComponents();
 
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
 
    authService.login.and.returnValue(of({ accessToken: 'mockAccess', refreshToken: 'mockRefresh' }));
    authService.getUserRole.and.returnValue('admin');
 
    fixture.detectChanges();
  });
 
  it('should create the component', () => {
    expect(component).toBeTruthy();
  });
 
  it('should initialize the form with username and password controls', () => {
    expect(component.loginForm.contains('username')).toBeTrue();
    expect(component.loginForm.contains('password')).toBeTrue();
  });
 
  it('should mark form as invalid when fields are empty', fakeAsync(() => {
    component.loginForm.setValue({ username: '', password: '' });
    fixture.detectChanges();
    tick();
    expect(component.loginForm.invalid).toBeTrue();
  }));
 
  it('should call login and redirect to admin dashboard on success', fakeAsync(() => {
    spyOn(component['router'], 'navigate').and.returnValue(Promise.resolve(true));
    component.loginForm.setValue({ username: 'admin', password: 'admin123' });
    component.onSubmit();
    fixture.detectChanges();
    tick();
    expect(authService.login).toHaveBeenCalledWith({ username: 'admin', password: 'admin123' });
    expect(authService.storeTokens).toHaveBeenCalledWith('mockAccess', 'mockRefresh');
    expect(component.message).toContain('Successfully logged in');
  }));
 
  it('should redirect to policyholder dashboard if role is policyholder', fakeAsync(() => {
    authService.getUserRole.and.returnValue('policyholder');
    spyOn(component['router'], 'navigate').and.returnValue(Promise.resolve(true));
    component.loginForm.setValue({ username: 'user', password: 'pass' });
    component.onSubmit();
    fixture.detectChanges();
    tick();
    expect(component.message).toContain('Successfully logged in');
  }));
 
  it('should redirect to manager dashboard if role is manager', fakeAsync(() => {
    authService.getUserRole.and.returnValue('manager');
    spyOn(component['router'], 'navigate').and.returnValue(Promise.resolve(true));
    component.loginForm.setValue({ username: 'manager', password: 'pass' });
    component.onSubmit();
    fixture.detectChanges();
    tick();
    expect(component.message).toContain('Successfully logged in');
  }));
 
  it('should show unknown role message if role is unrecognized', fakeAsync(() => {
    authService.getUserRole.and.returnValue('guest');
    component.loginForm.setValue({ username: 'guest', password: 'guest123' });
    component.onSubmit();
    fixture.detectChanges();
    tick();
    expect(component.message).toContain('Unknown role');
  }));
 
  it('should show error message on login failure', fakeAsync(() => {
    authService.login.and.returnValue(
      throwError({ error: { message: 'Invalid credentials' } })
    );
    component.loginForm.setValue({ username: 'wrong', password: 'wrong' });
    component.onSubmit();
    fixture.detectChanges();
    tick();
    expect(component.message).toBe('Invalid credentials');
    expect(component.loading).toBeFalse();
  }));
 
  it('should set loading to true during login and false after', fakeAsync(() => {
    authService.login.and.returnValue(
      of({ accessToken: 'mockAccess', refreshToken: 'mockRefresh' }).pipe(delay(100))
    );
    component.loginForm.setValue({ username: 'user', password: 'pass' });
    component.onSubmit();
    expect(component.loading).toBeTrue(); // ✅ Immediately after submit
    tick(100); // ✅ Flush delayed observable
    expect(component.loading).toBeFalse(); // ✅ After response
  }));
});