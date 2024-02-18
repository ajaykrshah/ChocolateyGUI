// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public class ChocolateySource : IEquatable<ChocolateySource>
	{
		#region Constructors

		public ChocolateySource(ChocolateySource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			this.Id = source.Id;
			this.Value = source.Value;
			this.Disabled = source.Disabled;
			this.UserName = source.UserName;
			this.Password = source.Password;
			this.Priority = source.Priority;
			this.Certificate = source.Certificate;
			this.CertificatePassword = source.CertificatePassword;
			this.BypassProxy = source.BypassProxy;
			this.AllowSelfService = source.AllowSelfService;
			this.VisibleToAdminsOnly = source.VisibleToAdminsOnly;
		}

		public ChocolateySource()
		{
		}

		#endregion

		#region Properties

		public Boolean AllowSelfService { get; set; }
		public Boolean BypassProxy { get; set; }
		public String Certificate { get; set; }
		public String CertificatePassword { get; set; }
		public Boolean Disabled { get; set; }
		public Boolean HasCertificatePassword => !String.IsNullOrWhiteSpace(this.CertificatePassword);
		public Boolean HasPassword => !String.IsNullOrWhiteSpace(this.Password);
		public String Id { get; set; }
		public String Password { get; set; }
		public Int32 Priority { get; set; }
		public String UserName { get; set; }
		public String Value { get; set; }
		public Boolean VisibleToAdminsOnly { get; set; }

		#endregion

		#region object overrides

		public override Boolean Equals(Object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}

			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((ChocolateySource)obj);
		}

		public override Int32 GetHashCode()
		{
			unchecked
			{
				var hashCode = this.Id?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (this.Value?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ this.Disabled.GetHashCode();
				hashCode = (hashCode * 397) ^ (this.UserName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (this.Password?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ this.Priority;
				hashCode = (hashCode * 397) ^ (this.Certificate?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (this.CertificatePassword?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ this.BypassProxy.GetHashCode();
				hashCode = (hashCode * 397) ^ this.AllowSelfService.GetHashCode();
				hashCode = (hashCode * 397) ^ this.VisibleToAdminsOnly.GetHashCode();
				return hashCode;
			}
		}

		#endregion

		#region IEquatable<ChocolateySource> implementation

		public Boolean Equals(ChocolateySource other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}

			if (object.ReferenceEquals(this, other))
			{
				return true;
			}

			return String.Equals(this.Id, other.Id)
			       && String.Equals(this.Value, other.Value)
			       && this.Disabled == other.Disabled
			       && String.Equals(this.UserName, other.UserName)
			       && String.Equals(this.Password, other.Password)
			       && this.Priority == other.Priority
			       && String.Equals(this.Certificate, other.Certificate)
			       && String.Equals(this.CertificatePassword, other.CertificatePassword)
			       && this.BypassProxy == other.BypassProxy
			       && this.AllowSelfService == other.AllowSelfService
			       && this.VisibleToAdminsOnly == other.VisibleToAdminsOnly;
		}

		#endregion
	}
}