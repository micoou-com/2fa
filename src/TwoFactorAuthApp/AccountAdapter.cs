using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using TwoFactorAuth.Core.Data;
using TwoFactorAuth.Core.Totp;

namespace TwoFactorAuthApp;

public sealed class AccountAdapter(IAccountStore store, Action<string> onDeleteRequest) : RecyclerView.Adapter
{
    private static readonly RefreshPayloadMarker RefreshPayload = new();
    private List<AccountEntry> _items = [];

    public void Reload()
    {
        _items = store.Load();
        NotifyDataSetChanged();
    }

    public void RefreshDisplay()
    {
        if (_items.Count == 0)
            return;
        NotifyItemRangeChanged(0, _items.Count, RefreshPayload);
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        View v = LayoutInflater.From(parent.Context)!.Inflate(Resource.Layout.item_account, parent, false)!;
        return new VH(v);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Java.Lang.Object> payloads)
    {
        if (payloads.Count > 0)
        {
            BindCodeAndTimer((VH)holder, _items[position]);
            return;
        }

        OnBindViewHolder(holder, position);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        var h = (VH)holder;
        AccountEntry a = _items[position];
        string title = string.IsNullOrEmpty(a.Issuer)
            ? a.AccountName
            : $"{a.Issuer} · {a.AccountName}";
        h.Title.Text = title;
        BindCodeAndTimer(h, a);
        h.Delete.SetOnClickListener(new DeleteClickListener(a.Id, onDeleteRequest));
        h.ItemView.SetOnLongClickListener(new LongDeleteListener(a.Id, onDeleteRequest));
    }

    private static void BindCodeAndTimer(VH h, AccountEntry a)
    {
        h.Code.Text = TotpGenerator.Generate(a.SecretBase32, a.Digits, a.PeriodSeconds);
        int remaining = TotpGenerator.SecondsRemaining(a.PeriodSeconds);
        h.Timer.Text = h.ItemView.Context!.GetString(Resource.String.timer_seconds, remaining);
    }

    public override int ItemCount => _items.Count;

    private sealed class RefreshPayloadMarker : Java.Lang.Object;

    private sealed class LongDeleteListener(string id, Action<string> act) : Java.Lang.Object, View.IOnLongClickListener
    {
        public bool OnLongClick(View? v)
        {
            act(id);
            return true;
        }
    }

    private sealed class DeleteClickListener(string id, Action<string> act) : Java.Lang.Object, View.IOnClickListener
    {
        public void OnClick(View? v) => act(id);
    }

    private sealed class VH(View itemView) : RecyclerView.ViewHolder(itemView)
    {
        public readonly TextView Title = itemView.FindViewById<TextView>(Resource.Id.text_title)!;
        public readonly TextView Code = itemView.FindViewById<TextView>(Resource.Id.text_code)!;
        public readonly TextView Timer = itemView.FindViewById<TextView>(Resource.Id.text_timer)!;
        public readonly ImageButton Delete = itemView.FindViewById<ImageButton>(Resource.Id.btn_delete)!;
    }
}
