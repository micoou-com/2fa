using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using TwoFactorAuth.Core.Data;
using TwoFactorAuth.Core.Totp;

namespace TwoFactorAuthApp;

public sealed class AccountAdapter(IAccountStore store, Action<string> onDeleteRequest) : RecyclerView.Adapter
{
    private List<AccountEntry> _items = [];

    public void Reload()
    {
        _items = store.Load();
        NotifyDataSetChanged();
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        View v = LayoutInflater.From(parent.Context)!.Inflate(Resource.Layout.item_account, parent, false)!;
        return new VH(v);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        var h = (VH)holder;
        AccountEntry a = _items[position];
        string title = string.IsNullOrEmpty(a.Issuer)
            ? a.AccountName
            : $"{a.Issuer} · {a.AccountName}";
        h.Title.Text = title;
        h.Code.Text = TotpGenerator.Generate(a.SecretBase32, a.Digits, a.PeriodSeconds);
        h.Timer.Text = $"{TotpGenerator.SecondsRemaining(a.PeriodSeconds)} s";
        h.ItemView.SetOnLongClickListener(new LongDeleteListener(a.Id, onDeleteRequest));
    }

    public override int ItemCount => _items.Count;

    private sealed class LongDeleteListener(string id, Action<string> act) : Java.Lang.Object, View.IOnLongClickListener
    {
        public bool OnLongClick(View? v)
        {
            act(id);
            return true;
        }
    }

    private sealed class VH(View itemView) : RecyclerView.ViewHolder(itemView)
    {
        public readonly TextView Title = itemView.FindViewById<TextView>(Resource.Id.text_title)!;
        public readonly TextView Code = itemView.FindViewById<TextView>(Resource.Id.text_code)!;
        public readonly TextView Timer = itemView.FindViewById<TextView>(Resource.Id.text_timer)!;
    }
}
