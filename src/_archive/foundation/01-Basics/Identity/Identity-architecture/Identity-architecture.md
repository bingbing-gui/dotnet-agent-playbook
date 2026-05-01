``` mermaid
graph LR;
  A[ASP.NET Core Identity] -->|用户管理| B[UserManager]
  A -->|角色管理| C[RoleManager]
  A -->|登录管理| D[SignInManager]
  A -->|用户存储| E[UserStore]
  A -->|角色存储| F[RoleStore]
  A -->|数据库| G[IdentityDbContext]
  A -->|身份验证| H[ClaimsPrincipal]
  A -->|认证方式| I[Authentication]

  %% UserManager 方法
  B --> B1[CreateAsync 创建用户]
  B --> B2[DeleteAsync 删除用户]
  B --> B3[UpdateAsync 更新用户]
  B --> B4[FindByIdAsync 按 ID 查找用户]
  B --> B5[FindByNameAsync 按用户名查找用户]
  B --> B6[GetUserIdAsync 获取用户ID]
  B --> B7[GetUserNameAsync 获取用户名]
  B --> B8[SetUserNameAsync 设置用户名]
  B --> B9[AddPasswordAsync 添加密码]
  B --> B10[ChangePasswordAsync 修改密码]
  B --> B11[RemovePasswordAsync 移除密码]
  B --> B12[CheckPasswordAsync 检查密码]
  B --> B13[GeneratePasswordResetTokenAsync 生成密码重置令牌]
  B --> B14[ResetPasswordAsync 重置密码]
  B --> B15[AddToRoleAsync 添加用户到角色]
  B --> B16[RemoveFromRoleAsync 移除用户角色]
  B --> B17[GetRolesAsync 获取用户角色]
  B --> B18[IsInRoleAsync 用户是否属于角色]
  B --> B19[AddClaimAsync 添加声明]
  B --> B20[RemoveClaimAsync 移除声明]
  B --> B21[GetClaimsAsync 获取用户声明]
  B --> B22[GenerateEmailConfirmationTokenAsync 生成邮箱确认令牌]
  B --> B23[ConfirmEmailAsync 确认邮箱]
  B --> B24[IsEmailConfirmedAsync 是否已确认邮箱]
  B --> B25[SetEmailAsync 设置邮箱]
  B --> B26[GetEmailAsync 获取邮箱]
  B --> B27[GenerateTwoFactorTokenAsync 生成 2FA 令牌]
  B --> B28[VerifyTwoFactorTokenAsync 验证 2FA 令牌]
  B --> B29[SetLockoutEnabledAsync 设置账户锁定]
  B --> B30[IsLockedOutAsync 账户是否被锁定]
  B --> B31[SetLockoutEndDateAsync 设置锁定到期时间]

  %% RoleManager 方法
  C --> C1[CreateAsync 创建角色]
  C --> C2[DeleteAsync 删除角色]
  C --> C3[UpdateAsync 更新角色]
  C --> C4[FindByIdAsync 按 ID 查找角色]
  C --> C5[FindByNameAsync 按名称查找角色]
  C --> C6[AddClaimAsync 添加角色声明]
  C --> C7[RemoveClaimAsync 移除角色声明]
  C --> C8[GetClaimsAsync 获取角色声明]

  %% SignInManager 方法
  D --> D1[PasswordSignInAsync 密码登录]
  D --> D2[SignInAsync 手动登录]
  D --> D3[SignOutAsync 注销用户]
  D --> D4[TwoFactorSignInAsync 双因素登录]
  D --> D5[ExternalLoginSignInAsync 外部登录]
  D --> D6[GetExternalLoginInfoAsync 获取外部登录信息]
  D --> D7[RefreshSignInAsync 刷新身份信息]
  D --> D8[IsTwoFactorEnabledAsync 是否启用 2FA]
  D --> D9[IsSignedIn 检查是否已登录]

  %% UserStore 方法
  E --> E1[CreateAsync 存储用户]
  E --> E2[DeleteAsync 删除用户]
  E --> E3[UpdateAsync 更新用户]
  E --> E4[FindByIdAsync 按 ID 查找用户]
  E --> E5[FindByNameAsync 按用户名查找用户]

  %% RoleStore 方法
  F --> F1[CreateAsync 存储角色]
  F --> F2[DeleteAsync 删除角色]
  F --> F3[UpdateAsync 更新角色]
  F --> F4[FindByIdAsync 按 ID 查找角色]
  F --> F5[FindByNameAsync 按名称查找角色]

  %% IdentityDbContext 数据表
  G --> G1[Users 用户表]
  G --> G2[Roles 角色表]
  G --> G3[UserRoles 用户-角色映射]
  G --> G4[UserClaims 用户声明]
  G --> G5[RoleClaims 角色声明]
  G --> G6[UserLogins 第三方登录]
  G --> G7[UserTokens 令牌存储]

  %% ClaimsPrincipal 方法
  H --> H1[FindFirst 获取指定类型声明]
  H --> H2[HasClaim 检查声明]

  %% Authentication 方式
  I --> I1[Cookies 基于 Cookie 认证]
  I --> I2[JWT 基于 JWT 认证]
  I --> I3[OAuth OAuth 认证]
  I --> I4[OpenID Connect OpenID 认证]
```