import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../components/navbar/navbar';

@Component({
  selector: 'app-policyholder-layout',
  imports: [RouterModule,NavbarComponent],
  templateUrl: './policyholder-layout.html',
  styleUrl: './policyholder-layout.css',
})
export class PolicyholderLayout {

}
