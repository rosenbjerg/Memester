import { Component, h } from "preact";
import { Meme } from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";

interface Props {
    threadId: number;
    memeId: number;
}

interface State {
    meme?: Meme;
}

export default class MemePage extends Component<Props, State> {
    componentDidMount() {
        ky.get(`/api/threads/${this.props.threadId}/${this.props.memeId}`)
            .json<Meme>()
            .then(meme => this.setState({ meme }));
    }

    render({ memeId, threadId }: Props, { meme }: State) {
        if (!meme) return <Loading />;

        return (
            <div>
                <h2>{meme.name}</h2>
                <video src={`/api/${threadId}/${memeId}/video`} />
            </div>
        );
    }
}
