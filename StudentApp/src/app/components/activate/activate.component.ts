import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ActivationService } from '../../services/assignment.service';

@Component({
  selector: 'app-activate',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="activate-wrapper">
      <div class="activate-card">

        <!-- Loading -->
        <div *ngIf="loading" class="state-msg">
          <div class="spinner"></div>
          <p>Verifying your activation link…</p>
        </div>

        <!-- Already activated -->
        <div *ngIf="!loading && alreadyActivated" class="state-msg">
          <div class="icon">✅</div>
          <h2>Already Activated</h2>
          <p>This account has already been activated. Please <a routerLink="/login">log in</a>.</p>
        </div>

        <!-- Token error -->
        <div *ngIf="!loading && tokenError" class="state-msg error">
          <div class="icon">❌</div>
          <h2>Invalid Link</h2>
          <p>{{ tokenError }}</p>
          <p>Please contact your administrator for a new activation link.</p>
        </div>

        <!-- Activation form -->
        <div *ngIf="!loading && !alreadyActivated && !tokenError && userInfo">
          <div class="card-header">
            <div class="role-badge" [class]="userInfo.role.toLowerCase()">{{ userInfo.role }}</div>
            <h1>Welcome, {{ userInfo.fullName }}!</h1>
            <p class="subtitle">Set your password to activate your account.</p>
            <p class="email-display">{{ userInfo.email }}</p>
          </div>

          <form (ngSubmit)="onSubmit()" #form="ngForm" class="activate-form">
            <div class="field-group">
              <label for="newPassword">New Password</label>
              <input id="newPassword" type="password" [(ngModel)]="newPassword"
                name="newPassword" required minlength="8"
                placeholder="Minimum 8 characters" />
            </div>

            <div class="field-group">
              <label for="confirmPassword">Confirm Password</label>
              <input id="confirmPassword" type="password" [(ngModel)]="confirmPassword"
                name="confirmPassword" required
                placeholder="Repeat your password" />
              <span class="hint error-hint" *ngIf="confirmPassword && newPassword !== confirmPassword">
                Passwords do not match
              </span>
            </div>

            <div class="strength-bar">
              <div class="bar" [style.width.%]="passwordStrength" [class]="strengthClass"></div>
            </div>
            <p class="strength-label">{{ strengthLabel }}</p>

            <div *ngIf="errorMsg" class="alert error">{{ errorMsg }}</div>

            <button type="submit" class="btn-activate"
              [disabled]="submitting || newPassword !== confirmPassword || newPassword.length < 8">
              <span *ngIf="!submitting">Activate Account</span>
              <span *ngIf="submitting" class="spinner-inline"></span>
            </button>
          </form>
        </div>

        <!-- Success state -->
        <div *ngIf="activated" class="state-msg success">
          <div class="icon">🎉</div>
          <h2>Account Activated!</h2>
          <p>Redirecting you to your dashboard…</p>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .activate-wrapper {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 2rem;
    }
    .activate-card {
      background: #fff;
      border-radius: 20px;
      padding: 2.5rem;
      width: 100%;
      max-width: 460px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.2);
    }
    .card-header { text-align: center; margin-bottom: 2rem; }
    .card-header h1 { font-size: 1.6rem; color: #1a1a2e; margin: 0.5rem 0; }
    .subtitle { color: #666; margin: 0; }
    .email-display { font-size: 0.9rem; color: #888; margin-top: 0.25rem; }
    .role-badge {
      display: inline-block; padding: 4px 14px; border-radius: 99px;
      font-size: 0.8rem; font-weight: 600; text-transform: uppercase;
      letter-spacing: 0.05em; margin-bottom: 0.75rem;
    }
    .role-badge.teacher { background: #dbeafe; color: #1e40af; }
    .role-badge.student { background: #d1fae5; color: #065f46; }
    .activate-form { display: flex; flex-direction: column; gap: 1.2rem; }
    .field-group { display: flex; flex-direction: column; gap: 0.4rem; }
    .field-group label { font-weight: 600; color: #374151; font-size: 0.9rem; }
    .field-group input {
      padding: 0.75rem 1rem; border: 1.5px solid #e5e7eb; border-radius: 10px;
      font-size: 1rem; outline: none; transition: border-color 0.2s;
    }
    .field-group input:focus { border-color: #667eea; }
    .hint { font-size: 0.8rem; }
    .error-hint { color: #ef4444; }
    .strength-bar {
      height: 6px; background: #f3f4f6; border-radius: 99px; overflow: hidden;
    }
    .bar { height: 100%; border-radius: 99px; transition: width 0.3s, background 0.3s; }
    .bar.weak   { background: #ef4444; }
    .bar.medium { background: #f59e0b; }
    .bar.strong { background: #10b981; }
    .strength-label { font-size: 0.8rem; color: #6b7280; margin: 0; text-align: right; }
    .btn-activate {
      padding: 0.9rem; background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff; border: none; border-radius: 12px; font-size: 1rem;
      font-weight: 600; cursor: pointer; transition: opacity 0.2s;
    }
    .btn-activate:disabled { opacity: 0.5; cursor: not-allowed; }
    .alert { padding: 0.75rem 1rem; border-radius: 8px; font-size: 0.9rem; }
    .alert.error { background: #fee2e2; color: #dc2626; }
    .state-msg { text-align: center; padding: 1rem 0; }
    .state-msg .icon { font-size: 3rem; margin-bottom: 0.5rem; }
    .state-msg h2 { color: #1a1a2e; margin: 0 0 0.5rem; }
    .state-msg p { color: #6b7280; }
    .state-msg a { color: #667eea; font-weight: 600; }
    .state-msg.success .icon { animation: pop 0.4s ease; }
    .spinner { width: 40px; height: 40px; border: 4px solid #e5e7eb;
      border-top-color: #667eea; border-radius: 50%; animation: spin 0.8s linear infinite;
      margin: 1rem auto; }
    .spinner-inline { display: inline-block; width: 18px; height: 18px;
      border: 3px solid rgba(255,255,255,0.4); border-top-color: #fff;
      border-radius: 50%; animation: spin 0.8s linear infinite; vertical-align: middle; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @keyframes pop  { 0%,100%{ transform: scale(1); } 50%{ transform: scale(1.2); } }
  `]
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
