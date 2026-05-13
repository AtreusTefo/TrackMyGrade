import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ActivationService } from '../../services/assignment.service';

@Component({
  selector: 'app-activate',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './activate.component.html',
  styleUrl: './activate.component.css'
})

export class ActivateComponent implements OnInit {
  token        = '';
  role         = '';
  userInfo: any = null;
  loading      = true;
  tokenError   = '';
  alreadyActivated = false;
  activated    = false;
  submitting   = false;
  errorMsg     = '';
  newPassword     = '';
  confirmPassword = '';
  showPassword = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private activationService: ActivationService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
    this.role  = this.route.snapshot.queryParamMap.get('role')  || '';

    if (!this.token || !this.role) {
      this.tokenError = 'Missing token or role in the activation link.';
      this.loading = false;
      return;
    }

    this.activationService.checkStatus(this.token, this.role).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.isActivated) { this.alreadyActivated = true; return; }
        this.userInfo = res;
      },
      error: (err) => {
        this.loading = false;
        this.tokenError = err?.error || 'Invalid or expired activation link.';
      }
    });
  }

  get passwordStrength(): number {
    const p = this.newPassword;
    if (!p) return 0;
    let score = 0;
    if (p.length >= 8)  score += 33;
    if (/[A-Z]/.test(p) && /[0-9]/.test(p)) score += 34;
    if (/[^A-Za-z0-9]/.test(p)) score += 33;
    return score;
  }

  get strengthClass(): string {
    const s = this.passwordStrength;
    if (s <= 33) return 'weak';
    if (s <= 66) return 'medium';
    return 'strong';
  }

  get strengthLabel(): string {
    const s = this.passwordStrength;
    if (!this.newPassword) return '';
    if (s <= 33) return 'Weak password';
    if (s <= 66) return 'Medium strength';
    return 'Strong password ✓';
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.newPassword !== this.confirmPassword) { return; }
    if (this.submitting) { return; } // Prevent double-submission
    this.submitting = true;
    this.errorMsg   = '';

    this.activationService.activate({
      role: this.role,
      activationToken: this.token,
      newPassword: this.newPassword,
      confirmPassword: this.confirmPassword
    }).subscribe({
      next: (res) => {
        // Store JWT in the correct localStorage key
        const key = this.role.toLowerCase() === 'teacher' ? 'teacher_token' : 'student_token';
        localStorage.setItem(key, res.token);
        localStorage.setItem(`${this.role.toLowerCase()}_user`, JSON.stringify(res));
        this.submitting = false;
        this.activated  = true;
        // Navigate after a short delay to allow UI to update
        setTimeout(() => {
          if (res.dashboard) {
            this.router.navigateByUrl(res.dashboard).catch(() => {
              this.router.navigate(['/login']);
            });
          }
        }, 1800);
      },
      error: (err) => {
        this.submitting = false;
        this.errorMsg = err?.error || 'Activation failed. Please try again.';
      }
    });
  }
}
